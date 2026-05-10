using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Chapters;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/chapters")]
public class ChaptersController : ControllerBase
{
    private readonly IChapterService _chapterService;

    public ChaptersController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpGet("book/{bookId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<IReadOnlyCollection<ChapterDto>>>> GetByBook(Guid bookId, CancellationToken cancellationToken)
    {
        var response = await _chapterService.GetByBookAsync(bookId, cancellationToken);
        return Ok(ResponseEnvelope<IReadOnlyCollection<ChapterDto>>.Success(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseEnvelope<ChapterDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _chapterService.GetByIdAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<ChapterDto>.Success(response));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<ChapterDto>>> Create(UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        var response = await _chapterService.CreateAsync(request, cancellationToken);
        return Ok(ResponseEnvelope<ChapterDto>.Success(response));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<ChapterDto>>> Update(Guid id, UpsertChapterRequest request, CancellationToken cancellationToken)
    {
        var response = await _chapterService.UpdateAsync(id, request, cancellationToken);
        return Ok(ResponseEnvelope<ChapterDto>.Success(response));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _chapterService.DeleteAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
