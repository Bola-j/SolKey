using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SolKey.Application.Common;

namespace SolKey.API.Middleware;

public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        if (!context.ModelState.IsValid)
        {
            errors.AddRange(context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Validation error."
                    : error.ErrorMessage));
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Validation error."
                        : error.ErrorMessage));
            }
        }

        if (errors.Count > 0)
        {
            var response = ResponseEnvelope<object>.Failure(errors.Distinct(), "Validation failed.");
            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
