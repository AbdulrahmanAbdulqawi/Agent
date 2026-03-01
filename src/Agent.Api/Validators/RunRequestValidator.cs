using Agent.Api.Controllers;
using FluentValidation;

namespace Agent.Api.Validators;

public class RunRequestValidator : AbstractValidator<RunRequest>
{
    public RunRequestValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty().WithMessage("Goal is required")
            .MinimumLength(10).WithMessage("Goal must be at least 10 characters")
            .MaximumLength(5000).WithMessage("Goal must not exceed 5000 characters");

        RuleFor(x => x.RepositoryUrl)
            .Must(BeValidGitHubUrl)
            .When(x => !string.IsNullOrEmpty(x.RepositoryUrl))
            .WithMessage("Repository URL must be a valid GitHub URL (e.g., https://github.com/owner/repo)");

        RuleFor(x => x.Ref)
            .MaximumLength(256).WithMessage("Ref must not exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Ref));
    }

    private static bool BeValidGitHubUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
               uri.Scheme == "https";
    }
}
