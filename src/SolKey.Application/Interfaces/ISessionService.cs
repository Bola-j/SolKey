using SolKey.Application.DTOs.Sessions;
using SolKey.Application.Common;

namespace SolKey.Application.Interfaces;

public interface ISessionService
{
    Task<PagedResponse<SessionDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<SessionDto> CreateAsync(Guid teacherId, CreateSessionRequest request, CancellationToken cancellationToken);
    Task PurchaseAsync(Guid userId, PurchaseSessionRequest request, CancellationToken cancellationToken);
}
