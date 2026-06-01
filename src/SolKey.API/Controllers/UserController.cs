using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Users;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public Task<ActionResult<ResponseEnvelope<UserProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _userService.GetProfileAsync(userId, cancellationToken));
    }

    [HttpPut("profile")]
    public Task<ActionResult<ResponseEnvelope<UserProfileResponse>>> UpdateProfile(UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        return ExecuteAsync(() => _userService.UpdateProfileAsync(userId, request, cancellationToken));
    }
}
