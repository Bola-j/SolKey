namespace SolKey.Application.DTOs.Videos;

public record UploadVideoRequest(
    string Title,
    string BlobPath,
    bool IsPremium,
    string Type,
    Guid? QuestionId,
    Guid? SessionId);
