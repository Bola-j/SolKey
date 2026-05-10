using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("verify-teacher/{teacherId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<object>>> VerifyTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        await _adminService.ApproveTeacherAsync(teacherId, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [HttpPost("approve-video/{videoId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<object>>> ApproveVideo(Guid videoId, CancellationToken cancellationToken)
    {
        await _adminService.ApproveVideoAsync(videoId, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [HttpPost("approve-payment/{paymentId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<object>>> ApprovePayment(Guid paymentId, CancellationToken cancellationToken)
    {
        await _adminService.ApprovePaymentAsync(paymentId, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
