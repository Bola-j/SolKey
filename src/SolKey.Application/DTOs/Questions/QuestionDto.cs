namespace SolKey.Application.DTOs.Questions;

public record QuestionDto(Guid Id, Guid LessonId, string Text, IReadOnlyCollection<string> Tags);
