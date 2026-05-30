using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Identity;
using SolKey.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace SolKey.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SolKeyDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;

    private const string EmailVerificationPurpose = "email_verification";

    public AuthService(
        SolKeyDbContext dbContext,
        JwtTokenService jwtTokenService,
        JwtOptions jwtOptions,
        IEmailSender emailSender,
        EmailOptions emailOptions)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions;
        _emailSender = emailSender;
        _emailOptions = emailOptions;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        if (request.IsTeacher && string.IsNullOrWhiteSpace(request.Bio))
        {
            throw new InvalidOperationException("Teachers must provide a bio.");
        }

        var role = request.IsTeacher ? UserRole.Teacher : UserRole.Student;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Bio = request.Bio,
            Role = role,
            IsEmailVerified = false,
            IsVerifiedTeacher = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.Email
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SendVerificationEmailAsync(user, enforceCooldown: false, cancellationToken);

        return await CreateAuthResponseAsync(user, request.DeviceId, request.DeviceName, "", cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        return await CreateAuthResponseAsync(user, request.DeviceId, request.DeviceName, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens.Include(t => t.User)
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token is null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Refresh token invalid.");
        }

        if (!token.Session.IsActive || token.Session.DeviceId != request.DeviceId)
        {
            throw new InvalidOperationException("Session invalid.");
        }

        token.IsRevoked = true;
        token.ModifiedAt = DateTime.UtcNow;
        token.ModifiedBy = token.User.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(token.User, token.Session.DeviceId, token.Session.DeviceName, ipAddress, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens.Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token is null)
        {
            return;
        }

        token.IsRevoked = true;
        token.Session.IsActive = false;
        token.ModifiedAt = DateTime.UtcNow;
        token.ModifiedBy = token.Session.UserId.ToString();
        token.Session.ModifiedAt = DateTime.UtcNow;
        token.Session.ModifiedBy = token.Session.UserId.ToString();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Invalid or expired verification token.");
        }

        var tokenHash = HashToken(token);
        var verificationToken = await _dbContext.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.Purpose == EmailVerificationPurpose,
                cancellationToken);

        if (verificationToken is null)
        {
            throw new InvalidOperationException("Invalid or expired verification token.");
        }

        var now = DateTime.UtcNow;

        if (verificationToken.ConsumedAt is not null)
        {
            if (verificationToken.User.IsEmailVerified)
            {
                return;
            }

            throw new InvalidOperationException("Invalid or expired verification token.");
        }

        if (verificationToken.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Invalid or expired verification token.");
        }

        if (!verificationToken.User.IsEmailVerified)
        {
            verificationToken.User.IsEmailVerified = true;
            verificationToken.User.ModifiedAt = now;
            verificationToken.User.ModifiedBy = verificationToken.User.Email;
        }

        verificationToken.ConsumedAt = now;
        verificationToken.ModifiedAt = now;
        verificationToken.ModifiedBy = verificationToken.User.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResendVerificationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        await SendVerificationEmailAsync(user, enforceCooldown: true, cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, string deviceId, string deviceName, string ipAddress, CancellationToken cancellationToken)
    {
        var hasSubscription = await _dbContext.Subscriptions.AnyAsync(s => s.UserId == user.Id && s.IsActive && s.EndDate > DateTime.UtcNow, cancellationToken);
        var limit = JwtTokenService.GetDeviceLimit(user, hasSubscription);
        var activeSessions = await _dbContext.UserSessions
            .Where(s => s.UserId == user.Id && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count >= limit)
        {
            var oldest = activeSessions.First();
            oldest.IsActive = false;
            oldest.ModifiedAt = DateTime.UtcNow;
            oldest.ModifiedBy = user.Email;
        }

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = deviceId,
            DeviceName = deviceName,
            IPAddress = ipAddress,
            IsActive = true,
            LastSeenAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Email
        };

        var refreshDays = _jwtOptions.RefreshTokenDays > 0 ? _jwtOptions.RefreshTokenDays : 7;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SessionId = session.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Email
        };

        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);

        _dbContext.UserSessions.Add(session);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, expiresAt, refreshToken.Token, refreshToken.ExpiresAt);
    }

    private async Task SendVerificationEmailAsync(User user, bool enforceCooldown, CancellationToken cancellationToken)
    {
        if (user.IsEmailVerified)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (enforceCooldown)
        {
            var cooldownMinutes = _emailOptions.ResendCooldownMinutes > 0
                ? _emailOptions.ResendCooldownMinutes
                : 60;

            var lastCreatedAt = await _dbContext.EmailVerificationTokens
                .Where(t => t.UserId == user.Id && t.Purpose == EmailVerificationPurpose)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => (DateTime?)t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastCreatedAt.HasValue && lastCreatedAt.Value.AddMinutes(cooldownMinutes) > now)
            {
                return;
            }
        }

        var rawToken = await CreateEmailVerificationTokenAsync(user, cancellationToken);
        var link = BuildVerificationLink(rawToken);

        var subject = "Verify your SolKey email";
        var body = $@"<p>Hi {user.FirstName},</p>
    <p>Please verify your email address by clicking the link below:</p>
    <p><a href=""{link}"">Verify email</a></p>
    <p>If you did not create this account, you can safely ignore this email.</p>";

        await _emailSender.SendAsync(user.Email, subject, body, cancellationToken);
    }

    private async Task<string> CreateEmailVerificationTokenAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var activeTokens = await _dbContext.EmailVerificationTokens
            .Where(t => t.UserId == user.Id
                && t.Purpose == EmailVerificationPurpose
                && t.ConsumedAt == null
                && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.ConsumedAt = now;
            token.ModifiedAt = now;
            token.ModifiedBy = user.Email;
        }

        var rawToken = GenerateToken();
        var tokenHash = HashToken(rawToken);
        var expiryHours = _emailOptions.TokenExpiryHours > 0 ? _emailOptions.TokenExpiryHours : 24;

        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(expiryHours),
            Purpose = EmailVerificationPurpose,
            CreatedAt = now,
            CreatedBy = user.Email
        };

        _dbContext.EmailVerificationTokens.Add(verificationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    private string BuildVerificationLink(string token)
    {
        var baseUrl = _emailOptions.VerificationBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Verification base URL is not configured.");
        }

        return $"{baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(token)}";
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
