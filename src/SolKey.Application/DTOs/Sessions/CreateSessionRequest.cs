namespace SolKey.Application.DTOs.Sessions;

public record CreateSessionRequest(string Title, string Description, decimal Price, int AccessDurationDays);
