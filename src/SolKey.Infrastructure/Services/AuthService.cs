using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Identity;
using SolKey.Infrastructure.Persistence;
using System.Security.Cryptography;

namespace SolKey.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SolKeyDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(SolKeyDbContext dbContext, JwtTokenService jwtTokenService, JwtOptions jwtOptions)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions;
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

    public async Task VerifyEmailAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.IsEmailVerified = true;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = user.Email;
        await _dbContext.SaveChangesAsync(cancellationToken);
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
}
