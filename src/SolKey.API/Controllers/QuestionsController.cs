using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Questions;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController : ApiControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan VoteCooldown = TimeSpan.FromSeconds(30);

    public QuestionsController(IQuestionService questionService, IMemoryCache cache)
    {
        _questionService = questionService;
        _cache = cache;
    }

    [HttpPost("ask")]
    public Task<ActionResult<ResponseEnvelope<QuestionDto>>> Ask(AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _questionService.AskAsync(userId, request, cancellationToken));
    }

    [HttpPost("answer")]
    [Authorize(Roles = "Teacher")]
    public Task<ActionResult<ResponseEnvelope<object>>> Answer(AnswerQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _questionService.AnswerAsync(userId, request, cancellationToken));
    }

    [HttpPost("vote")]
    public Task<ActionResult<ResponseEnvelope<object>>> Vote(VoteRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var cacheKey = $"vote:{userId:N}:{request.AnswerId:N}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            var response = ResponseEnvelope<object>.Failure("You are voting too quickly. Please wait and try again.");
            return Task.FromResult<ActionResult<ResponseEnvelope<object>>>(
                StatusCode(StatusCodes.Status429TooManyRequests, response));
        }

        _cache.Set(cacheKey, true, VoteCooldown);
        return ExecuteAsync(() => _questionService.VoteAsync(userId, request, cancellationToken));
    }
}
