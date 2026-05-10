namespace SolKey.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
    Task<string> GenerateSignedUrlAsync(string blobPath, TimeSpan expiresIn, CancellationToken cancellationToken);
}
