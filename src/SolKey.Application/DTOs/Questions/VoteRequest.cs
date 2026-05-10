namespace SolKey.Application.DTOs.Questions;

public record VoteRequest(Guid AnswerId, bool IsUpvote);
