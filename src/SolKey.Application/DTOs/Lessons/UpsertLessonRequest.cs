namespace SolKey.Application.DTOs.Lessons;

public record UpsertLessonRequest(Guid ChapterId, string Title, int Order);
