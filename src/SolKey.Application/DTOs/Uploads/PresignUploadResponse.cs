namespace SolKey.Application.DTOs.Uploads;

public class PresignUploadResponse
{
    public Guid TicketId { get; set; }
    public string UploadUrl { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
