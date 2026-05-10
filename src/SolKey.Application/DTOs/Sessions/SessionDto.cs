namespace SolKey.Application.DTOs.Sessions;

public record SessionDto(Guid Id, string Title, string Description, decimal Price, bool IsApproved, int AccessDurationDays);
