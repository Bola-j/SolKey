using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string Purpose { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
