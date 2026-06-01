using SolKey.Application.DTOs.Lessons;
using SolKey.Application.Common;

namespace SolKey.Application.Interfaces;

public interface ILessonService
{
    Task<PagedResponse<LessonDto>> GetByChapterAsync(Guid chapterId, int page, int pageSize, CancellationToken cancellationToken);
    Task<LessonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LessonDto> CreateAsync(UpsertLessonRequest request, CancellationToken cancellationToken);
    Task<LessonDto> UpdateAsync(Guid id, UpsertLessonRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
