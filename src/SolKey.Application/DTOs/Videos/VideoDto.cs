namespace SolKey.Application.DTOs.Videos;

public record VideoDto(Guid Id, string Title, string Type, bool IsPremium, bool IsApproved);
