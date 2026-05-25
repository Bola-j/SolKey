using FluentValidation;
using Microsoft.Extensions.Hosting;
using SolKey.Application.DTOs.Auth;

namespace SolKey.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IHostEnvironment environment)
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty();
        RuleFor(request => request.DeviceId).MaximumLength(200);
        RuleFor(request => request.DeviceName).MaximumLength(200);

        if (!environment.IsDevelopment())
        {
            RuleFor(request => request.DeviceId).NotEmpty();
            RuleFor(request => request.DeviceName).NotEmpty();
        }
    }
}
