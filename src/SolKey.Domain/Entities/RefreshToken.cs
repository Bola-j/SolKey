using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public User User { get; set; } = null!;
    public UserSession Session { get; set; } = null!;
}
