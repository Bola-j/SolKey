using FluentValidation;
using SolKey.Application.DTOs.Sessions;

namespace SolKey.Application.Validators;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
        RuleFor(request => request.AccessDurationDays).GreaterThan(0);
    }
}
