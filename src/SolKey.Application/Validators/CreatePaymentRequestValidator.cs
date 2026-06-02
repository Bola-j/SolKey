using FluentValidation;
using SolKey.Application.DTOs.Payments;
using SolKey.Domain.Enums;

namespace SolKey.Application.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(request => request.Amount).GreaterThan(0);
        RuleFor(request => request.Type).NotEmpty();
        RuleFor(request => request.ScreenshotPath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.SessionId)
            .NotNull()
            .When(request => Enum.TryParse<PaymentType>(request.Type, true, out var type) && type == PaymentType.SessionPurchase)
            .WithMessage("SessionId is required for session purchase payments.");
        RuleFor(request => request.SessionId)
            .Null()
            .When(request => Enum.TryParse<PaymentType>(request.Type, true, out var type) && type == PaymentType.QaSubscription)
            .WithMessage("SessionId is only allowed for session purchase payments.");
    }
}
