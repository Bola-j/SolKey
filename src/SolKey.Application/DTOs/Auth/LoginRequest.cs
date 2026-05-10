namespace SolKey.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password, string DeviceId, string DeviceName);
