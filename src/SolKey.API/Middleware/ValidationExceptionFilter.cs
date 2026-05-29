using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SolKey.Application.Common;

namespace SolKey.API.Middleware;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ValidationException validationException)
        {
            return;
        }

        var errors = validationException.Errors
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? $"{error.PropertyName} is invalid."
                : error.ErrorMessage)
            .ToArray();

        var response = ResponseEnvelope<object>.Failure(errors, "Validation failed.");
        context.Result = new BadRequestObjectResult(response);
        context.ExceptionHandled = true;
    }
}
