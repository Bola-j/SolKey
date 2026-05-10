using Microsoft.IdentityModel.Tokens;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SolKey.Infrastructure.Identity;

public class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("role", user.Role.ToString()),
            new("isVerifiedTeacher", user.IsVerifiedTeacher.ToString()),
            new("isEmailVerified", user.IsEmailVerified.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public static int GetDeviceLimit(User user, bool hasSubscription)
    {
        return user.Role switch
        {
            UserRole.Teacher => 3,
            UserRole.Student => hasSubscription ? 2 : 1,
            _ => 3
        };
    }
}
