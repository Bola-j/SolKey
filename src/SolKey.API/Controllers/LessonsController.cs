using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Lessons;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet("chapter/{chapterId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<IReadOnlyCollection<LessonDto>>>> GetByChapter(Guid chapterId, CancellationToken cancellationToken)
    {
        var response = await _lessonService.GetByChapterAsync(chapterId, cancellationToken);
        return Ok(ResponseEnvelope<IReadOnlyCollection<LessonDto>>.Success(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseEnvelope<LessonDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _lessonService.GetByIdAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<LessonDto>.Success(response));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<LessonDto>>> Create(UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        var response = await _lessonService.CreateAsync(request, cancellationToken);
        return Ok(ResponseEnvelope<LessonDto>.Success(response));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<LessonDto>>> Update(Guid id, UpsertLessonRequest request, CancellationToken cancellationToken)
    {
        var response = await _lessonService.UpdateAsync(id, request, cancellationToken);
        return Ok(ResponseEnvelope<LessonDto>.Success(response));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _lessonService.DeleteAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
