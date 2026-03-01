using Agent.Api.Controllers;
using Agent.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using AgentEntity = Agent.Core.Models.Agent;

namespace Agent.Tests;

public class AgentValidatorTests
{
    private readonly AgentValidator _validator = new();

    [Fact]
    public void Validate_ValidAgent_ShouldNotHaveErrors()
    {
        var agent = new AgentEntity
        {
            Name = "Test Agent",
            Provider = "Claude"
        };

        var result = _validator.TestValidate(agent);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        var agent = new AgentEntity
        {
            Name = "",
            Provider = "Claude"
        };

        var result = _validator.TestValidate(agent);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveError()
    {
        var agent = new AgentEntity
        {
            Name = new string('a', 101),
            Provider = "Claude"
        };

        var result = _validator.TestValidate(agent);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Agent name must not exceed 100 characters");
    }

    [Theory]
    [InlineData("Claude")]
    [InlineData("OpenAI")]
    [InlineData("Cursor")]
    [InlineData("claude")]
    [InlineData("OPENAI")]
    public void Validate_ValidProvider_ShouldNotHaveError(string provider)
    {
        var agent = new AgentEntity
        {
            Name = "Test",
            Provider = provider
        };

        var result = _validator.TestValidate(agent);

        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidProvider")]
    [InlineData("GPT4")]
    public void Validate_InvalidProvider_ShouldHaveError(string provider)
    {
        var agent = new AgentEntity
        {
            Name = "Test",
            Provider = provider
        };

        var result = _validator.TestValidate(agent);

        result.ShouldHaveValidationErrorFor(x => x.Provider);
    }
}

public class RunRequestValidatorTests
{
    private readonly RunRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        var request = new RunRequest
        {
            Goal = "Implement a REST API endpoint for user management"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGoal_ShouldHaveError()
    {
        var request = new RunRequest
        {
            Goal = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Goal);
    }

    [Fact]
    public void Validate_GoalTooShort_ShouldHaveError()
    {
        var request = new RunRequest
        {
            Goal = "Fix bug"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Goal)
            .WithErrorMessage("Goal must be at least 10 characters");
    }

    [Theory]
    [InlineData("https://github.com/owner/repo")]
    [InlineData("https://github.com/owner/repo.git")]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ValidRepositoryUrl_ShouldNotHaveError(string? url)
    {
        var request = new RunRequest
        {
            Goal = "Implement a feature for the application",
            RepositoryUrl = url
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.RepositoryUrl);
    }

    [Theory]
    [InlineData("http://github.com/owner/repo")]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("not-a-url")]
    public void Validate_InvalidRepositoryUrl_ShouldHaveError(string url)
    {
        var request = new RunRequest
        {
            Goal = "Implement a feature for the application",
            RepositoryUrl = url
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RepositoryUrl);
    }
}
