using SolKey.Application.DTOs.Subscriptions;

namespace SolKey.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionStatusResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken);
    Task CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken);
}
