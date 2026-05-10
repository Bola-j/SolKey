using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Books;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly SolKeyDbContext _dbContext;

    public BookService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<BookDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var books = await _dbContext.Books.AsNoTracking().ToListAsync(cancellationToken);
        return books.Select(book => new BookDto(book.Id, book.Title, book.Description, book.CoverImage)).ToList();
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
