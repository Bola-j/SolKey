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
public class SubscriptionController : ApiControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("status")]
    public Task<ActionResult<ResponseEnvelope<SubscriptionStatusResponse>>> Status(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _subscriptionService.GetStatusAsync(userId, cancellationToken));
    }

    [HttpPost("payment")]
    public Task<ActionResult<ResponseEnvelope<object>>> Payment(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _subscriptionService.CreatePaymentAsync(userId, request, cancellationToken));
    }
}
