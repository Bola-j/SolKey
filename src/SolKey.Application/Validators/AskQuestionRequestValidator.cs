using FluentValidation;
using SolKey.Application.DTOs.Questions;

namespace SolKey.Application.Validators;

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(request => request.LessonId).NotEmpty();
        RuleFor(request => request.Text).NotEmpty().MaximumLength(4000);
    }
}
