namespace SolKey.Application.DTOs.Auth;

public record RefreshRequest(string RefreshToken, string DeviceId);
