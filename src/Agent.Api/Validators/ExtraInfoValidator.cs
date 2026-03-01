using Agent.Core.Models;
using FluentValidation;

namespace Agent.Api.Validators;

public class ExtraInfoValidator : AbstractValidator<ExtraInfo>
{
    public ExtraInfoValidator()
    {
        RuleFor(x => x.ProjectPath)
            .MaximumLength(500).WithMessage("Project path must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ProjectPath));

        RuleFor(x => x.TechStack)
            .MaximumLength(500).WithMessage("Tech stack must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.TechStack));

        RuleFor(x => x.Constraints)
            .MaximumLength(2000).WithMessage("Constraints must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Constraints));

        RuleFor(x => x.RelevantFiles)
            .MaximumLength(2000).WithMessage("Relevant files must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.RelevantFiles));

        RuleFor(x => x.Notes)
            .MaximumLength(5000).WithMessage("Notes must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
