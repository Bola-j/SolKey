using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class UploadTicket : BaseEntity
{
    public Guid UserId { get; set; }
    public string BlobPath { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public long? Size { get; set; }
    public string? ContentType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public User? User { get; set; }
}
