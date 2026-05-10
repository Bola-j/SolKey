using SolKey.Application.DTOs.Lessons;

namespace SolKey.Application.Interfaces;

public interface ILessonService
{
    Task<IReadOnlyCollection<LessonDto>> GetByChapterAsync(Guid chapterId, CancellationToken cancellationToken);
    Task<LessonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LessonDto> CreateAsync(UpsertLessonRequest request, CancellationToken cancellationToken);
    Task<LessonDto> UpdateAsync(Guid id, UpsertLessonRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
