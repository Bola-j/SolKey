using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class ExplanationSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid TeacherId { get; set; }
    public int AccessDurationDays { get; set; } = 14;
    public bool IsApproved { get; set; }

    public User Teacher { get; set; } = null!;
    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<SessionPurchase> Purchases { get; set; } = new List<SessionPurchase>();
}
