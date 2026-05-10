using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Chapters;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class ChapterService : IChapterService
{
    private readonly SolKeyDbContext _dbContext;

    public ChapterService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ChapterDto>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var chapters = await _dbContext.Chapters.AsNoTracking()
            .Where(c => c.BookId == bookId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        return chapters.Select(chapter => new ChapterDto(chapter.Id, chapter.BookId, chapter.Title, chapter.Order)).ToList();
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
