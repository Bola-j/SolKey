using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; } = null!;
}
