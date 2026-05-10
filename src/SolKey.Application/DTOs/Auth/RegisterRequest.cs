namespace SolKey.Application.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    string? Bio,
    bool IsTeacher,
    string DeviceId,
    string DeviceName);
