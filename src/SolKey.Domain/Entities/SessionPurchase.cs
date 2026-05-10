using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class SessionPurchase : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
    public ExplanationSession Session { get; set; } = null!;
}
