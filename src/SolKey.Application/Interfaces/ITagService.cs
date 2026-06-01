using SolKey.Application.Common;
using SolKey.Application.DTOs.Tags;

namespace SolKey.Application.Interfaces;

public interface ITagService
{
    Task<PagedResponse<TagDto>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken);
    Task<TagDto> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
