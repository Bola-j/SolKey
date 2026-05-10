using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var response = await _authService.LoginAsync(request, ipAddress, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var response = await _authService.RefreshAsync(request, ipAddress, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [HttpPost("verify-email/{userId:guid}")]
    public async Task<ActionResult<ResponseEnvelope<object>>> VerifyEmail(Guid userId, CancellationToken cancellationToken)
    {
        await _authService.VerifyEmailAsync(userId, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
