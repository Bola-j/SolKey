using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.DTOs.Uploads;
using SolKey.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadsController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost("presign")]
    [Authorize]
    public async Task<IActionResult> Presign([FromBody] PresignUploadRequest request, CancellationToken cancellationToken)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
        {
            return Unauthorized();
        }

        var result = await _uploadService.CreatePresignedUploadAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}
