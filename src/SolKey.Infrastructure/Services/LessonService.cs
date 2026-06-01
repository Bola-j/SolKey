using Microsoft.EntityFrameworkCore;
using System;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Lessons;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class LessonService : ILessonService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SolKeyDbContext _dbContext;

    public LessonService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<LessonDto>> GetByChapterAsync(Guid chapterId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _dbContext.Lessons.AsNoTracking()
            .Where(l => l.ChapterId == chapterId);

        var totalCount = await query.CountAsync(cancellationToken);
        var lessons = await query
            .OrderBy(l => l.Order)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(lesson => new LessonDto(lesson.Id, lesson.ChapterId, lesson.Title, lesson.Order))
            .ToListAsync(cancellationToken);

        return PagedResponse<LessonDto>.Success(lessons, normalizedPage, normalizedPageSize, totalCount);
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
