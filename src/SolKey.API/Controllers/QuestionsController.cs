using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Questions;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<ResponseEnvelope<QuestionDto>>> Ask(AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _questionService.AskAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<QuestionDto>.Success(response));
    }

    [HttpPost("answer")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Answer(AnswerQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _questionService.AnswerAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [HttpPost("vote")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Vote(VoteRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _questionService.VoteAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
