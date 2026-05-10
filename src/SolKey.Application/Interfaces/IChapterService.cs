using SolKey.Application.DTOs.Chapters;

namespace SolKey.Application.Interfaces;

public interface IChapterService
{
    Task<IReadOnlyCollection<ChapterDto>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken);
    Task<ChapterDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChapterDto> CreateAsync(UpsertChapterRequest request, CancellationToken cancellationToken);
    Task<ChapterDto> UpdateAsync(Guid id, UpsertChapterRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
