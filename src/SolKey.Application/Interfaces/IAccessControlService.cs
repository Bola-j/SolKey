namespace SolKey.Application.Interfaces;

public interface IAccessControlService
{
    Task<bool> CanAccessVideoAsync(Guid userId, Guid videoId, CancellationToken cancellationToken);
    Task<bool> CanAccessQuestionAnswersAsync(Guid userId, Guid questionId, CancellationToken cancellationToken);
    Task<bool> CanPurchaseSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
}
