using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Payments;
using SolKey.Application.DTOs.Subscriptions;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly SolKeyDbContext _dbContext;

    public SubscriptionService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionStatusResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await _dbContext.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow, cancellationToken);

        return subscription is null
            ? new SubscriptionStatusResponse(false, null, null)
            : new SubscriptionStatusResponse(true, subscription.StartDate, subscription.EndDate);
    }

    public async Task CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentType>(request.Type, true, out var type))
        {
            throw new InvalidOperationException("Invalid payment type.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = type == PaymentType.SessionPurchase ? request.SessionId : null,
            Amount = request.Amount,
            Type = type,
            ScreenshotPath = request.ScreenshotPath,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
