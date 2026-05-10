namespace SolKey.Application.DTOs.Lessons;

public record LessonDto(Guid Id, Guid ChapterId, string Title, int Order);
