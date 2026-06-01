using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using SolKey.Application.DTOs.Uploads;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class UploadService : IUploadService
{
    private readonly IAmazonS3 _s3;
    private readonly SolKeyDbContext _db;
    private readonly string _bucketName;

    public UploadService(IAmazonS3 s3, SolKeyDbContext db, IConfiguration config)
    {
        _s3 = s3;
        _db = db;
        _bucketName = config["Storage:BucketName"] ?? string.Empty;
    }

    public async Task<PresignUploadResponse> CreatePresignedUploadAsync(Guid userId, PresignUploadRequest request, CancellationToken cancellationToken = default)
    {
        var key = $"uploads/{userId:N}/{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid():N}-{request.FileName}";

        var expires = TimeSpan.FromMinutes(request.ExpiresInMinutes <= 0 ? 15 : request.ExpiresInMinutes);

        var preSignedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expires),
            Verb = HttpVerb.PUT,
        };

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            preSignedRequest.Headers.ContentType = request.ContentType;
        }

        var url = _s3.GetPreSignedURL(preSignedRequest);

        var ticket = new UploadTicket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BlobPath = key,
            Purpose = request.Purpose ?? string.Empty,
            Size = request.Size,
            ContentType = request.ContentType,
            ExpiresAt = DateTime.UtcNow.Add(expires),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _db.UploadTickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        return new PresignUploadResponse
        {
            TicketId = ticket.Id,
            UploadUrl = url,
            BlobPath = key,
            ExpiresAt = ticket.ExpiresAt
        };
    }

    public async Task<bool> ValidateAndConsumeTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.UploadTickets.FindAsync(new object[] { ticketId }, cancellationToken);
        if (ticket == null) return false;
        if (ticket.IsUsed) return false;
        if (ticket.UserId != userId) return false;
        if (ticket.ExpiresAt < DateTime.UtcNow) return false;

        ticket.IsUsed = true;
        _db.UploadTickets.Update(ticket);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
