using Microsoft.EntityFrameworkCore;
using System;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Books;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class BookService : IBookService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SolKeyDbContext _dbContext;

    public BookService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<BookDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _dbContext.Books.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var books = await query
            .OrderBy(b => b.Title)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(book => new BookDto(book.Id, book.Title, book.Description, book.CoverImage))
            .ToListAsync(cancellationToken);

        return PagedResponse<BookDto>.Success(books, normalizedPage, normalizedPageSize, totalCount);
    }

    public async Task<BookDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Book not found.");

        return new BookDto(book.Id, book.Title, book.Description, book.CoverImage);
    }

    public async Task<BookDto> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CoverImage = request.CoverImage,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new BookDto(book.Id, book.Title, book.Description, book.CoverImage);
    }

    public async Task<BookDto> UpdateAsync(Guid id, UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Book not found.");

        book.Title = request.Title;
        book.Description = request.Description;
        book.CoverImage = request.CoverImage;
        book.ModifiedAt = DateTime.UtcNow;
        book.ModifiedBy = "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new BookDto(book.Id, book.Title, book.Description, book.CoverImage);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Book not found.");

        book.IsDeleted = true;
        book.ModifiedAt = DateTime.UtcNow;
        book.ModifiedBy = "system";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
