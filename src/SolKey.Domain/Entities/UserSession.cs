using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastSeenAt { get; set; }

    public User User { get; set; } = null!;
}
