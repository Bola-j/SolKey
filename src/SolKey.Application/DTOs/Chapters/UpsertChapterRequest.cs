namespace SolKey.Application.DTOs.Chapters;

public record UpsertChapterRequest(Guid BookId, string Title, int Order);
