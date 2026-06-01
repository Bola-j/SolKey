using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Uploads;
using SolKey.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ApiControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadsController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost("presign")]
    [Authorize]
    public Task<ActionResult<ResponseEnvelope<PresignUploadResponse>>> Presign([FromBody] PresignUploadRequest request, CancellationToken cancellationToken)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
        {
            var response = ResponseEnvelope<PresignUploadResponse>.Failure("Unauthorized.");
            return Task.FromResult<ActionResult<ResponseEnvelope<PresignUploadResponse>>>(
                StatusCode(StatusCodes.Status401Unauthorized, response));
        }

        return ExecuteAsync(() => _uploadService.CreatePresignedUploadAsync(userId, request, cancellationToken));
    }
}
