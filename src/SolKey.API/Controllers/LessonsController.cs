using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Lessons;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ApiControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet("chapter/{chapterId:guid}")]
    public Task<ActionResult<ResponseEnvelope<PagedResponse<LessonDto>>>> GetByChapter(
        Guid chapterId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => _lessonService.GetByChapterAsync(chapterId, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public Task<ActionResult<ResponseEnvelope<LessonDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _lessonService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<LessonDto>>> Create(UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _lessonService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<LessonDto>>> Update(Guid id, UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _lessonService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _lessonService.DeleteAsync(id, cancellationToken));
    }
}
