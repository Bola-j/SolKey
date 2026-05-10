using SolKey.Application.DTOs.Users;

namespace SolKey.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken);
}
