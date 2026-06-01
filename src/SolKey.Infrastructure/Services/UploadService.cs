using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using SolKey.Application.DTOs.Uploads;
using System;
using System.IO;
using System.Linq;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class UploadService : IUploadService
{
    private readonly IAmazonS3 _s3;
    private readonly SolKeyDbContext _db;
    private readonly string _bucketName;
    private readonly IConfiguration _config;

    public UploadService(IAmazonS3 s3, SolKeyDbContext db, IConfiguration config)
    {
        _s3 = s3;
        _db = db;
        _config = config;
        _bucketName = config["Storage:BucketName"] ?? string.Empty;
    }

    public async Task<PresignUploadResponse> CreatePresignedUploadAsync(Guid userId, PresignUploadRequest request, CancellationToken cancellationToken = default)
    {
        // Server-side constraints from configuration
        var maxSizeBytes = 0L;
        var maxSizeStr = _config["Upload:MaxSizeBytes"];
        if (!string.IsNullOrWhiteSpace(maxSizeStr) && long.TryParse(maxSizeStr, out var parsedMax))
        {
            maxSizeBytes = parsedMax;
        }

        if (request.Size.HasValue && maxSizeBytes > 0 && request.Size.Value > maxSizeBytes)
        {
            throw new InvalidOperationException($"Requested upload size {request.Size.Value} exceeds maximum allowed {maxSizeBytes} bytes.");
        }

        var allowedCsv = _config["Upload:AllowedContentTypes"];
        if (!string.IsNullOrWhiteSpace(allowedCsv))
        {
            var allowed = allowedCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(request.ContentType) && allowed.Count > 0 && !allowed.Contains(request.ContentType))
            {
                throw new InvalidOperationException($"Content type '{request.ContentType}' is not allowed.");
            }
        }

        var safeFileName = Path.GetFileName(request.FileName ?? string.Empty);
        var key = $"uploads/{userId:N}/{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid():N}-{safeFileName}";

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

    public async Task<bool> ValidateAndConsumeTicketAsync(Guid ticketId, Guid userId, string? expectedBlobPath, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.UploadTickets.FindAsync(new object[] { ticketId }, cancellationToken);
        if (ticket == null) return false;
        if (ticket.IsUsed) return false;
        if (ticket.UserId != userId) return false;
        if (ticket.ExpiresAt < DateTime.UtcNow) return false;
        if (!string.IsNullOrWhiteSpace(expectedBlobPath) && !string.Equals(ticket.BlobPath, expectedBlobPath, StringComparison.Ordinal))
        {
            return false;
        }

        ticket.IsUsed = true;
        _db.UploadTickets.Update(ticket);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
