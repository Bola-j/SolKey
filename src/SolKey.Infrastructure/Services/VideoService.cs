using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Videos;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class VideoService : IVideoService
{
    private readonly SolKeyDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IAccessControlService _accessControlService;
    private readonly IUploadService _uploadService;

    public VideoService(SolKeyDbContext dbContext, IStorageService storageService, IAccessControlService accessControlService, IUploadService uploadService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _accessControlService = accessControlService;
        _uploadService = uploadService;
    }

    public async Task<VideoDto> UploadAsync(Guid teacherId, UploadVideoRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        if (teacher.Role != UserRole.Teacher)
        {
            throw new InvalidOperationException("Only teachers can upload videos.");
        }

        if (!teacher.IsVerifiedTeacher)
        {
            throw new InvalidOperationException("Only verified teachers can upload videos.");
        }

        if (!Enum.TryParse<VideoType>(request.Type, true, out var type))
        {
            throw new InvalidOperationException("Invalid video type.");
        }

        if (request.UploadTicketId is null)
        {
            throw new InvalidOperationException("Upload ticket required.");
        }

        var ticketValid = await _uploadService.ValidateAndConsumeTicketAsync(request.UploadTicketId.Value, teacherId, request.BlobPath, cancellationToken);
        if (!ticketValid)
        {
            throw new InvalidOperationException("Invalid or expired upload ticket.");
        }

        var blobExists = await _storageService.BlobExistsAsync(request.BlobPath, cancellationToken);
        if (!blobExists)
        {
            throw new InvalidOperationException("Upload blob not found.");
        }

        var video = new Video
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Type = type,
            BlobPath = request.BlobPath,
            IsPremium = request.IsPremium,
            IsApproved = false,
            TeacherId = teacherId,
            QuestionId = request.QuestionId,
            SessionId = request.SessionId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = teacher.Email
        };

        _dbContext.Videos.Add(video);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new VideoDto(video.Id, video.Title, video.Type.ToString(), video.IsPremium, video.IsApproved);
    }

    public async Task<SecureVideoResponse> GetSecureUrlAsync(Guid userId, Guid videoId, CancellationToken cancellationToken)
    {
        var canAccess = await _accessControlService.CanAccessVideoAsync(userId, videoId, cancellationToken);
        if (!canAccess)
        {
            throw new InvalidOperationException("Access denied.");
        }

        var video = await _dbContext.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        var url = await _storageService.GenerateSignedUrlAsync(video.BlobPath, TimeSpan.FromMinutes(15), cancellationToken);
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var watermarkText = string.Join(" | ", new[] { user.Email, user.PhoneNumber }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return new SecureVideoResponse(url, watermarkText);
    }
}
