using Agent.Core.Models;
using FluentValidation;

namespace Agent.Api.Validators;

public class ExtraInfoValidator : AbstractValidator<ExtraInfo>
{
    public ExtraInfoValidator()
    {
        RuleFor(x => x.ProjectPath)
            .MaximumLength(500).WithMessage("Project path must be 500 characters or less")
            .When(x => !string.IsNullOrEmpty(x.ProjectPath));

        RuleFor(x => x.TechStack)
            .MaximumLength(1000).WithMessage("Tech stack must be 1,000 characters or less")
            .When(x => !string.IsNullOrEmpty(x.TechStack));

        RuleFor(x => x.Constraints)
            .MaximumLength(2000).WithMessage("Constraints must be 2,000 characters or less")
            .When(x => !string.IsNullOrEmpty(x.Constraints));

        RuleFor(x => x.RelevantFiles)
            .MaximumLength(5000).WithMessage("Relevant files must be 5,000 characters or less")
            .When(x => !string.IsNullOrEmpty(x.RelevantFiles));

        RuleFor(x => x.Notes)
            .MaximumLength(5000).WithMessage("Notes must be 5,000 characters or less")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
