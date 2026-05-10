using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Videos;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/videos")]
[Authorize]
public class VideosController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ResponseEnvelope<VideoDto>>> Upload(UploadVideoRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _videoService.UploadAsync(userId, request, cancellationToken);
        return Ok(ResponseEnvelope<VideoDto>.Success(response));
    }

    [HttpGet("{id:guid}/secure-url")]
    public async Task<ActionResult<ResponseEnvelope<SecureVideoResponse>>> GetSecureUrl(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _videoService.GetSecureUrlAsync(userId, id, cancellationToken);
        return Ok(ResponseEnvelope<SecureVideoResponse>.Success(response));
    }
}
