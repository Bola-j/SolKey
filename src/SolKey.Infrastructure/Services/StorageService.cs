using Amazon.S3;
using Amazon.S3.Model;
using SolKey.Application.Interfaces;

namespace SolKey.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public StorageService(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var key = $"uploads/{Guid.NewGuid():N}/{fileName}";
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return key;
    }

    public async Task<string> GenerateSignedUrlAsync(string blobPath, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = blobPath,
            Expires = DateTime.UtcNow.Add(expiresIn)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task<bool> BlobExistsAsync(string blobPath, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = blobPath
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
