using SolKey.Application.DTOs.Books;
using SolKey.Application.Common;

namespace SolKey.Application.Interfaces;

public interface IBookService
{
    Task<PagedResponse<BookDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BookDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BookDto> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken);
    Task<BookDto> UpdateAsync(Guid id, UpsertBookRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
