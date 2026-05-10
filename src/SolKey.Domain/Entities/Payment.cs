using SolKey.Domain.Common;
using SolKey.Domain.Enums;

namespace SolKey.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public string ScreenshotPath { get; set; } = string.Empty;
    public bool IsApproved { get; set; }

    public User User { get; set; } = null!;
}
