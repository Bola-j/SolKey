using SolKey.Application.DTOs.Videos;

namespace SolKey.Application.Interfaces;

public interface IVideoService
{
    Task<VideoDto> UploadAsync(Guid teacherId, UploadVideoRequest request, CancellationToken cancellationToken);
    Task<SecureVideoResponse> GetSecureUrlAsync(Guid userId, Guid videoId, CancellationToken cancellationToken);
}
