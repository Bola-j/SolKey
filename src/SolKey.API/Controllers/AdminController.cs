using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("verify-teacher/{teacherId:guid}")]
    public Task<ActionResult<ResponseEnvelope<object>>> VerifyTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _adminService.ApproveTeacherAsync(teacherId, cancellationToken));
    }

    [HttpPost("approve-video/{videoId:guid}")]
    public Task<ActionResult<ResponseEnvelope<object>>> ApproveVideo(Guid videoId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _adminService.ApproveVideoAsync(videoId, cancellationToken));
    }

    [HttpPost("approve-payment/{paymentId:guid}")]
    public Task<ActionResult<ResponseEnvelope<object>>> ApprovePayment(Guid paymentId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _adminService.ApprovePaymentAsync(paymentId, cancellationToken));
    }
}
