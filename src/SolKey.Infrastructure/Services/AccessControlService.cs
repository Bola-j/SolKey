using Microsoft.EntityFrameworkCore;
using SolKey.Application.Interfaces;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class AccessControlService : IAccessControlService
{
    private readonly SolKeyDbContext _dbContext;

    public AccessControlService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CanAccessVideoAsync(Guid userId, Guid videoId, CancellationToken cancellationToken)
    {
        var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);
        if (video is null || !video.IsApproved)
        {
            return false;
        }

        if (!video.IsPremium)
        {
            return true;
        }

        if (video.Type == VideoType.QuestionSolution)
        {
            return await HasActiveSubscriptionAsync(userId, cancellationToken);
        }

        if (video.SessionId is null)
        {
            return false;
        }

        var hasPurchase = await _dbContext.SessionPurchases.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.SessionId == video.SessionId && p.ExpiresAt > DateTime.UtcNow, cancellationToken);

        return hasPurchase;
    }

    public async Task<bool> CanAccessQuestionAnswersAsync(Guid userId, Guid questionId, CancellationToken cancellationToken)
    {
        return await HasActiveSubscriptionAsync(userId, cancellationToken);
    }

    public async Task<bool> CanPurchaseSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        return !await _dbContext.SessionPurchases.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.SessionId == sessionId && p.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    private Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Subscriptions.AsNoTracking()
            .AnyAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow, cancellationToken);
    }
}
