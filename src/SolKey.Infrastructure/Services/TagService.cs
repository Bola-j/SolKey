using Microsoft.EntityFrameworkCore;
using System;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Tags;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class TagService : ITagService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly SolKeyDbContext _dbContext;

    public TagService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<TagDto>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = _dbContext.Tags.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var tags = await query
            .OrderBy(t => t.Name)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);

        return PagedResponse<TagDto>.Success(tags, normalizedPage, normalizedPageSize, totalCount);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tag name is required.");
        }

        var exists = await _dbContext.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Tag already exists.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto(tag.Id, tag.Name);
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Tag not found.");

        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tag name is required.");
        }

        var exists = await _dbContext.Tags.AnyAsync(t => t.Id != id && t.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Tag already exists.");
        }

        tag.Name = name;
        tag.ModifiedAt = DateTime.UtcNow;
        tag.ModifiedBy = "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TagDto(tag.Id, tag.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Tag not found.");

        tag.IsDeleted = true;
        tag.ModifiedAt = DateTime.UtcNow;
        tag.ModifiedBy = "system";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
