using SolKey.Application.DTOs.Uploads;

namespace SolKey.Application.Interfaces;

public interface IUploadService
{
    Task<PresignUploadResponse> CreatePresignedUploadAsync(Guid userId, PresignUploadRequest request, CancellationToken cancellationToken = default);
    Task<bool> ValidateAndConsumeTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default);
}
