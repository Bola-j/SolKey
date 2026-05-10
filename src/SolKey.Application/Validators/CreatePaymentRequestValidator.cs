using FluentValidation;
using SolKey.Application.DTOs.Payments;

namespace SolKey.Application.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(request => request.Amount).GreaterThan(0);
        RuleFor(request => request.Type).NotEmpty();
        RuleFor(request => request.ScreenshotPath).NotEmpty().MaximumLength(500);
    }
}
