using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Videos;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/videos")]
[Authorize]
public class VideosController : ApiControllerBase
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    public Task<ActionResult<ResponseEnvelope<VideoDto>>> Upload(UploadVideoRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _videoService.UploadAsync(userId, request, cancellationToken));
    }

    [HttpGet("{id:guid}/secure-url")]
    public Task<ActionResult<ResponseEnvelope<SecureVideoResponse>>> GetSecureUrl(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _videoService.GetSecureUrlAsync(userId, id, cancellationToken));
    }
}
