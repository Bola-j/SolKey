using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHostEnvironment _environment;

    private const string DefaultDeviceId = "manual-test";
    private const string DefaultDeviceName = "Manual Test Client";

    public AuthController(IAuthService authService, IHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }


    [Authorize]
[HttpGet("test-auth")]
public IActionResult TestAuth()
{
    return Ok(User.Identity?.Name);
}

    [HttpPost("register")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var normalizedRequest = ApplyDeviceDefaults(request);
        var response = await _authService.RegisterAsync(normalizedRequest, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var normalizedRequest = ApplyDeviceDefaults(request);
        var response = await _authService.LoginAsync(normalizedRequest, ipAddress, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ResponseEnvelope<AuthResponse>>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var normalizedRequest = ApplyDeviceDefaults(request);
        var response = await _authService.RefreshAsync(normalizedRequest, ipAddress, cancellationToken);
        return Ok(ResponseEnvelope<AuthResponse>.Success(response));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var normalizedRequest = ApplyDeviceDefaults(request);
        await _authService.LogoutAsync(normalizedRequest, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult<ResponseEnvelope<object>>> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _authService.VerifyEmailAsync(token, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [Authorize]
    [HttpPost("resend-verification")]
    public async Task<ActionResult<ResponseEnvelope<object>>> ResendVerification(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _authService.ResendVerificationAsync(userId, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }

    [Authorize]
    [HttpGet("whoami")]
    public ActionResult<ResponseEnvelope<object>> WhoAmI()
    {
        var claims = User.Claims.Select(claim => new { claim.Type, claim.Value }).ToList();
        var hasAuthHeader = Request.Headers.ContainsKey("Authorization");
        return Ok(ResponseEnvelope<object>.Success(new { hasAuthHeader, claims }));
    }

    private RegisterRequest ApplyDeviceDefaults(RegisterRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return request;
        }

        var deviceId = GetDeviceId(request.DeviceId);
        var deviceName = GetDeviceName(request.DeviceName);

        return request with
        {
            DeviceId = deviceId,
            DeviceName = deviceName
        };
    }

    private LoginRequest ApplyDeviceDefaults(LoginRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return request;
        }

        var deviceId = GetDeviceId(request.DeviceId);
        var deviceName = GetDeviceName(request.DeviceName);

        return request with
        {
            DeviceId = deviceId,
            DeviceName = deviceName
        };
    }

    private RefreshRequest ApplyDeviceDefaults(RefreshRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return request;
        }

        var deviceId = GetDeviceId(request.DeviceId);

        return request with
        {
            DeviceId = deviceId
        };
    }

    private LogoutRequest ApplyDeviceDefaults(LogoutRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return request;
        }

        var deviceId = GetDeviceId(request.DeviceId);

        return request with
        {
            DeviceId = deviceId
        };
    }

    private string GetDeviceId(string? deviceId)
    {
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            return deviceId;
        }

        var headerValue = Request.Headers["X-Device-Id"].ToString();
        return string.IsNullOrWhiteSpace(headerValue) ? DefaultDeviceId : headerValue;
    }

    private string GetDeviceName(string? deviceName)
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            return deviceName;
        }

        var headerValue = Request.Headers["X-Device-Name"].ToString();
        return string.IsNullOrWhiteSpace(headerValue) ? DefaultDeviceName : headerValue;
    }
}
