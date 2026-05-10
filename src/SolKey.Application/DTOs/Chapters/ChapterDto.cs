namespace SolKey.Application.DTOs.Chapters;

public record ChapterDto(Guid Id, Guid BookId, string Title, int Order);
