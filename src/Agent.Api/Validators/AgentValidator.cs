using FluentValidation;
using AgentEntity = Agent.Core.Models.Agent;

namespace Agent.Api.Validators;

public class AgentValidator : AbstractValidator<AgentEntity>
{
    private static readonly string[] ValidProviders = ["Claude", "OpenAI", "Cursor"];

    public AgentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Agent name is required")
            .MaximumLength(100).WithMessage("Agent name must not exceed 100 characters");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(p => ValidProviders.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Provider must be one of: {string.Join(", ", ValidProviders)}");

        RuleFor(x => x.WorkspacePath)
            .MaximumLength(500).WithMessage("Workspace path must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.WorkspacePath));
    }
}
