namespace SolKey.Application.DTOs.Subscriptions;

public record SubscriptionStatusResponse(bool IsActive, DateTime? StartDate, DateTime? EndDate);
