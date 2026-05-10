using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Lessons;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class LessonService : ILessonService
{
    private readonly SolKeyDbContext _dbContext;

    public LessonService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<LessonDto>> GetByChapterAsync(Guid chapterId, CancellationToken cancellationToken)
    {
        var lessons = await _dbContext.Lessons.AsNoTracking()
            .Where(l => l.ChapterId == chapterId)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);

        return lessons.Select(lesson => new LessonDto(lesson.Id, lesson.ChapterId, lesson.Title, lesson.Order)).ToList();
    }

    public async Task<LessonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await _dbContext.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Lesson not found.");

        return new LessonDto(lesson.Id, lesson.ChapterId, lesson.Title, lesson.Order);
    }

    public async Task<LessonDto> CreateAsync(UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            ChapterId = request.ChapterId,
            Title = request.Title,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        _dbContext.Lessons.Add(lesson);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new LessonDto(lesson.Id, lesson.ChapterId, lesson.Title, lesson.Order);
    }

    public async Task<LessonDto> UpdateAsync(Guid id, UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Lesson not found.");

        lesson.ChapterId = request.ChapterId;
        lesson.Title = request.Title;
        lesson.Order = request.Order;
        lesson.ModifiedAt = DateTime.UtcNow;
        lesson.ModifiedBy = "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new LessonDto(lesson.Id, lesson.ChapterId, lesson.Title, lesson.Order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Lesson not found.");

        lesson.IsDeleted = true;
        lesson.ModifiedAt = DateTime.UtcNow;
        lesson.ModifiedBy = "system";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
