using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Payments;
using SolKey.Application.DTOs.Subscriptions;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize(Roles = "Student")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<ResponseEnvelope<SubscriptionStatusResponse>>> Status(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _subscriptionService.GetStatusAsync(userId, cancellationToken);
        return Ok(ResponseEnvelope<SubscriptionStatusResponse>.Success(response));
    }

    [HttpPost("payment")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Payment(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _subscriptionService.CreatePaymentAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
