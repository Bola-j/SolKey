using SolKey.Application.DTOs.Sessions;

namespace SolKey.Application.Interfaces;

public interface ISessionService
{
    Task<IReadOnlyCollection<SessionDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<SessionDto> CreateAsync(Guid teacherId, CreateSessionRequest request, CancellationToken cancellationToken);
    Task PurchaseAsync(Guid userId, PurchaseSessionRequest request, CancellationToken cancellationToken);
}
