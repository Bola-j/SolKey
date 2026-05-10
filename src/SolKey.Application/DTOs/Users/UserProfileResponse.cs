namespace SolKey.Application.DTOs.Users;

public record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? PhotoUrl,
    string? Bio,
    bool IsEmailVerified,
    bool IsVerifiedTeacher);
