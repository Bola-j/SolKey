using FluentValidation;
using SolKey.Application.DTOs.Videos;

namespace SolKey.Application.Validators;

public class UploadVideoRequestValidator : AbstractValidator<UploadVideoRequest>
{
    public UploadVideoRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.BlobPath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.Type).NotEmpty();
    }
}
