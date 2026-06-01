using Microsoft.EntityFrameworkCore;
using System;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Chapters;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class ChapterService : IChapterService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SolKeyDbContext _dbContext;

    public ChapterService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ChapterDto>> GetByBookAsync(Guid bookId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _dbContext.Chapters.AsNoTracking()
            .Where(c => c.BookId == bookId);

        var totalCount = await query.CountAsync(cancellationToken);
        var chapters = await query
            .OrderBy(c => c.Order)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(chapter => new ChapterDto(chapter.Id, chapter.BookId, chapter.Title, chapter.Order))
            .ToListAsync(cancellationToken);

        return PagedResponse<ChapterDto>.Success(chapters, normalizedPage, normalizedPageSize, totalCount);
    }

    public async Task<ChapterDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var chapter = await _dbContext.Chapters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Chapter not found.");

        return new ChapterDto(chapter.Id, chapter.BookId, chapter.Title, chapter.Order);
    }

    public async Task<ChapterDto> CreateAsync(UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            Title = request.Title,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        _dbContext.Chapters.Add(chapter);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ChapterDto(chapter.Id, chapter.BookId, chapter.Title, chapter.Order);
    }

    public async Task<ChapterDto> UpdateAsync(Guid id, UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        var chapter = await _dbContext.Chapters.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Chapter not found.");

        chapter.BookId = request.BookId;
        chapter.Title = request.Title;
        chapter.Order = request.Order;
        chapter.ModifiedAt = DateTime.UtcNow;
        chapter.ModifiedBy = "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ChapterDto(chapter.Id, chapter.BookId, chapter.Title, chapter.Order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var chapter = await _dbContext.Chapters.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Chapter not found.");

        chapter.IsDeleted = true;
        chapter.ModifiedAt = DateTime.UtcNow;
        chapter.ModifiedBy = "system";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
