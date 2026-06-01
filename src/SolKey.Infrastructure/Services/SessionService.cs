using Microsoft.EntityFrameworkCore;
using System;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Sessions;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class SessionService : ISessionService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SolKeyDbContext _dbContext;
    private readonly IAccessControlService _accessControlService;

    public SessionService(SolKeyDbContext dbContext, IAccessControlService accessControlService)
    {
        _dbContext = dbContext;
        _accessControlService = accessControlService;
    }

    public async Task<PagedResponse<SessionDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _dbContext.ExplanationSessions.AsNoTracking().Where(s => s.IsApproved);
        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .OrderBy(s => s.Title)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(s => new SessionDto(s.Id, s.Title, s.Description, s.Price, s.IsApproved, s.AccessDurationDays))
            .ToListAsync(cancellationToken);

        return PagedResponse<SessionDto>.Success(sessions, normalizedPage, normalizedPageSize, totalCount);
    }

    public async Task<SessionDto> CreateAsync(Guid teacherId, CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        if (teacher.Role != UserRole.Teacher)
        {
            throw new InvalidOperationException("Only teachers can create sessions.");
        }

        var session = new ExplanationSession
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            TeacherId = teacherId,
            AccessDurationDays = request.AccessDurationDays > 0 ? request.AccessDurationDays : 14,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = teacher.Email
        };

        _dbContext.ExplanationSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SessionDto(session.Id, session.Title, session.Description, session.Price, session.IsApproved, session.AccessDurationDays);
    }

    public async Task PurchaseAsync(Guid userId, PurchaseSessionRequest request, CancellationToken cancellationToken)
    {
        var canPurchase = await _accessControlService.CanPurchaseSessionAsync(userId, request.SessionId, cancellationToken);
        if (!canPurchase)
        {
            throw new InvalidOperationException("Session already purchased.");
        }

        var session = await _dbContext.ExplanationSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        var purchase = new SessionPurchase
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = request.SessionId,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(session.AccessDurationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        _dbContext.SessionPurchases.Add(purchase);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
