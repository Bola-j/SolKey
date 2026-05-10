using FluentValidation;
using SolKey.Application.DTOs.Auth;

namespace SolKey.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty();
        RuleFor(request => request.DeviceId).NotEmpty().MaximumLength(200);
        RuleFor(request => request.DeviceName).NotEmpty().MaximumLength(200);
    }
}
