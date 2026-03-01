using Agent.Api.Controllers;
using Agent.Api.Validators;
using Agent.Core.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using AgentEntity = Agent.Core.Models.Agent;

namespace Agent.Tests;

public class AgentValidatorTests
{
    private readonly AgentValidator _validator = new();

    [Fact]
    public void Validate_ValidAgent_PassesValidation()
    {
        var agent = new AgentEntity
        {
            Name = "Test Agent",
            Provider = "Claude"
        };

        var result = _validator.TestValidate(agent);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Claude")]
    [InlineData("OpenAI")]
    [InlineData("Cursor")]
    [InlineData("claude")]
    [InlineData("openai")]
    [InlineData("cursor")]
    public void Validate_ValidProvider_PassesValidation(string provider)
    {
        var agent = new AgentEntity { Name = "Test", Provider = provider };
        var result = _validator.TestValidate(agent);
        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void Validate_EmptyName_FailsValidation()
    {
        var agent = new AgentEntity { Name = "", Provider = "Claude" };
        var result = _validator.TestValidate(agent);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        var agent = new AgentEntity
        {
            Name = new string('a', 101),
            Provider = "Claude"
        };
        var result = _validator.TestValidate(agent);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyProvider_FailsValidation()
    {
        var agent = new AgentEntity { Name = "Test", Provider = "" };
        var result = _validator.TestValidate(agent);
        result.ShouldHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void Validate_InvalidProvider_FailsValidation()
    {
        var agent = new AgentEntity { Name = "Test", Provider = "InvalidProvider" };
        var result = _validator.TestValidate(agent);
        result.ShouldHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public void Validate_WorkspacePathTooLong_FailsValidation()
    {
        var agent = new AgentEntity
        {
            Name = "Test",
            Provider = "Claude",
            WorkspacePath = new string('a', 501)
        };
        var result = _validator.TestValidate(agent);
        result.ShouldHaveValidationErrorFor(x => x.WorkspacePath);
    }
}

public class RunRequestValidatorTests
{
    private readonly RunRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        var request = new RunRequest { Goal = "Implement feature X" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGoal_FailsValidation()
    {
        var request = new RunRequest { Goal = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Goal);
    }

    [Fact]
    public void Validate_GoalTooLong_FailsValidation()
    {
        var request = new RunRequest { Goal = new string('a', 10001) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Goal);
    }

    [Fact]
    public void Validate_ValidRepositoryUrl_PassesValidation()
    {
        var request = new RunRequest
        {
            Goal = "Test",
            RepositoryUrl = "https://github.com/user/repo"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.RepositoryUrl);
    }

    [Fact]
    public void Validate_InvalidRepositoryUrl_FailsValidation()
    {
        var request = new RunRequest
        {
            Goal = "Test",
            RepositoryUrl = "not-a-valid-url"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RepositoryUrl);
    }

    [Fact]
    public void Validate_RefTooLong_FailsValidation()
    {
        var request = new RunRequest
        {
            Goal = "Test",
            Ref = new string('a', 257)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ref);
    }
}

public class ExtraInfoValidatorTests
{
    private readonly ExtraInfoValidator _validator = new();

    [Fact]
    public void Validate_EmptyExtraInfo_PassesValidation()
    {
        var extraInfo = new ExtraInfo();
        var result = _validator.TestValidate(extraInfo);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidExtraInfo_PassesValidation()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/src/app",
            TechStack = "React, TypeScript",
            Constraints = "Must be accessible",
            RelevantFiles = "src/index.ts",
            Notes = "Important notes"
        };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ProjectPathTooLong_FailsValidation()
    {
        var extraInfo = new ExtraInfo { ProjectPath = new string('a', 501) };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldHaveValidationErrorFor(x => x.ProjectPath);
    }

    [Fact]
    public void Validate_TechStackTooLong_FailsValidation()
    {
        var extraInfo = new ExtraInfo { TechStack = new string('a', 1001) };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldHaveValidationErrorFor(x => x.TechStack);
    }

    [Fact]
    public void Validate_ConstraintsTooLong_FailsValidation()
    {
        var extraInfo = new ExtraInfo { Constraints = new string('a', 2001) };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldHaveValidationErrorFor(x => x.Constraints);
    }

    [Fact]
    public void Validate_RelevantFilesTooLong_FailsValidation()
    {
        var extraInfo = new ExtraInfo { RelevantFiles = new string('a', 5001) };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldHaveValidationErrorFor(x => x.RelevantFiles);
    }

    [Fact]
    public void Validate_NotesTooLong_FailsValidation()
    {
        var extraInfo = new ExtraInfo { Notes = new string('a', 5001) };
        var result = _validator.TestValidate(extraInfo);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
