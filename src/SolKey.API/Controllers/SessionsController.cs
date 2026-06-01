using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Sessions;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ApiControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public Task<ActionResult<ResponseEnvelope<PagedResponse<SessionDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => _sessionService.GetAllAsync(page, pageSize, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public Task<ActionResult<ResponseEnvelope<SessionDto>>> Create(CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _sessionService.CreateAsync(userId, request, cancellationToken));
    }

    [HttpPost("purchase")]
    [Authorize(Roles = "Student")]
    public Task<ActionResult<ResponseEnvelope<object>>> Purchase(PurchaseSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _sessionService.PurchaseAsync(userId, request, cancellationToken));
    }
}
