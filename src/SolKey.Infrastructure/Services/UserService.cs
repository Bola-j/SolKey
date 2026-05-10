using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Users;
using SolKey.Application.Interfaces;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly SolKeyDbContext _dbContext;

    public UserService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        return new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.PhotoUrl, user.Bio, user.IsEmailVerified, user.IsVerifiedTeacher);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.PhotoUrl = request.PhotoUrl;
        user.Bio = request.Bio;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = user.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.PhotoUrl, user.Bio, user.IsEmailVerified, user.IsVerifiedTeacher);
    }
}
