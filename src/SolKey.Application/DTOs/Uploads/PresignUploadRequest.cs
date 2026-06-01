namespace SolKey.Application.DTOs.Uploads;

public class PresignUploadRequest
{
    public required string FileName { get; set; }
    public string? ContentType { get; set; }
    public long? Size { get; set; }
    public string? Purpose { get; set; }
    public int ExpiresInMinutes { get; set; } = 15;
}
