using Agent.Api.Controllers;
using FluentValidation;

namespace Agent.Api.Validators;

public class RunRequestValidator : AbstractValidator<RunRequest>
{
    public RunRequestValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty().WithMessage("Goal is required")
            .MaximumLength(10000).WithMessage("Goal must be 10,000 characters or less");

        RuleFor(x => x.RepositoryUrl)
            .Must(BeValidUrl).WithMessage("Repository URL must be a valid GitHub URL")
            .When(x => !string.IsNullOrEmpty(x.RepositoryUrl));

        RuleFor(x => x.Ref)
            .MaximumLength(256).WithMessage("Ref must be 256 characters or less")
            .When(x => !string.IsNullOrEmpty(x.Ref));
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
