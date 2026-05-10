using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Sessions;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<ActionResult<ResponseEnvelope<IReadOnlyCollection<SessionDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _sessionService.GetAllAsync(cancellationToken);
        return Ok(ResponseEnvelope<IReadOnlyCollection<SessionDto>>.Success(response));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ResponseEnvelope<SessionDto>>> Create(CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _sessionService.CreateAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<SessionDto>.Success(response));
    }

    [HttpPost("purchase")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Purchase(PurchaseSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _sessionService.PurchaseAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
