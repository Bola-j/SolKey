namespace SolKey.Application.DTOs.Users;

public record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? PhotoUrl,
    string? Bio);
