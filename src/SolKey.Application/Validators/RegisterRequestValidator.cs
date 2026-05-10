using FluentValidation;
using SolKey.Application.DTOs.Auth;

namespace SolKey.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(8);
        RuleFor(request => request.Bio).MaximumLength(1000);
        RuleFor(request => request.DeviceId).NotEmpty().MaximumLength(200);
        RuleFor(request => request.DeviceName).NotEmpty().MaximumLength(200);
    }
}
