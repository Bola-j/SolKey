using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Tags;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ApiControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public Task<ActionResult<ResponseEnvelope<PagedResponse<TagDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => _tagService.GetPagedAsync(search, page, pageSize, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<TagDto>>> Create(CreateTagRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _tagService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<TagDto>>> Update(Guid id, UpdateTagRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _tagService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _tagService.DeleteAsync(id, cancellationToken));
    }
}
