using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Chapters;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/chapters")]
public class ChaptersController : ApiControllerBase
{
    private readonly IChapterService _chapterService;

    public ChaptersController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpGet("book/{bookId:guid}")]
    public Task<ActionResult<ResponseEnvelope<PagedResponse<ChapterDto>>>> GetByBook(
        Guid bookId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => _chapterService.GetByBookAsync(bookId, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public Task<ActionResult<ResponseEnvelope<ChapterDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _chapterService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<ChapterDto>>> Create(UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _chapterService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<ChapterDto>>> Update(Guid id, UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _chapterService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _chapterService.DeleteAsync(id, cancellationToken));
    }
}
