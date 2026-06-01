using SolKey.Application.DTOs.Chapters;
using SolKey.Application.Common;

namespace SolKey.Application.Interfaces;

public interface IChapterService
{
    Task<PagedResponse<ChapterDto>> GetByBookAsync(Guid bookId, int page, int pageSize, CancellationToken cancellationToken);
    Task<ChapterDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChapterDto> CreateAsync(UpsertChapterRequest request, CancellationToken cancellationToken);
    Task<ChapterDto> UpdateAsync(Guid id, UpsertChapterRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
