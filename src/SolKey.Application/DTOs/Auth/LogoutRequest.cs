namespace SolKey.Application.DTOs.Auth;

public record LogoutRequest(string RefreshToken, string DeviceId);
