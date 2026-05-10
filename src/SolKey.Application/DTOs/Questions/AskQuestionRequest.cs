namespace SolKey.Application.DTOs.Questions;

public record AskQuestionRequest(Guid LessonId, string Text, IReadOnlyCollection<string> Tags);
