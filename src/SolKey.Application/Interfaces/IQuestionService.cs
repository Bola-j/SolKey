using SolKey.Application.DTOs.Questions;

namespace SolKey.Application.Interfaces;

public interface IQuestionService
{
    Task<QuestionDto> AskAsync(Guid studentId, AskQuestionRequest request, CancellationToken cancellationToken);
    Task AnswerAsync(Guid teacherId, AnswerQuestionRequest request, CancellationToken cancellationToken);
    Task VoteAsync(VoteRequest request, CancellationToken cancellationToken);
}
