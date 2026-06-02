using Microsoft.EntityFrameworkCore;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly SolKeyDbContext _dbContext;

    public AdminService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ApproveTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        teacher.IsVerifiedTeacher = true;
        teacher.ModifiedAt = DateTime.UtcNow;
        teacher.ModifiedBy = "admin";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveVideoAsync(Guid videoId, CancellationToken cancellationToken)
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        video.IsApproved = true;
        video.ModifiedAt = DateTime.UtcNow;
        video.ModifiedBy = "admin";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApprovePaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.IsApproved)
        {
            return;
        }

        payment.IsApproved = true;
        payment.ModifiedAt = DateTime.UtcNow;
        payment.ModifiedBy = "admin";

        var utcNow = DateTime.UtcNow;

        if (payment.Type == PaymentType.QaSubscription)
        {
            var activeSubscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == payment.UserId && s.IsActive && s.EndDate > utcNow, cancellationToken);

            if (activeSubscription != null)
            {
                activeSubscription.EndDate = activeSubscription.EndDate.AddMonths(1);
                activeSubscription.ModifiedAt = utcNow;
                activeSubscription.ModifiedBy = "admin";
            }
            else
            {
                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = payment.UserId,
                    StartDate = utcNow,
                    EndDate = utcNow.AddMonths(1),
                    IsActive = true,
                    CreatedAt = utcNow,
                    CreatedBy = "admin"
                };
                _dbContext.Subscriptions.Add(subscription);
            }
        }
        else if (payment.Type == PaymentType.SessionPurchase)
        {
            if (payment.SessionId is null)
            {
                throw new InvalidOperationException("Session purchase payment is missing SessionId.");
            }

            var session = await _dbContext.ExplanationSessions
                .FirstOrDefaultAsync(s => s.Id == payment.SessionId.Value && s.IsApproved, cancellationToken)
                ?? throw new InvalidOperationException("Approved session not found.");

            var activePurchase = await _dbContext.SessionPurchases
                .FirstOrDefaultAsync(p => p.UserId == payment.UserId && p.SessionId == session.Id && p.ExpiresAt > utcNow, cancellationToken);

            if (activePurchase is null)
            {
                var purchase = new SessionPurchase
                {
                    Id = Guid.NewGuid(),
                    UserId = payment.UserId,
                    SessionId = session.Id,
                    PurchasedAt = utcNow,
                    ExpiresAt = utcNow.AddDays(session.AccessDurationDays),
                    CreatedAt = utcNow,
                    CreatedBy = "admin"
                };
                _dbContext.SessionPurchases.Add(purchase);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
