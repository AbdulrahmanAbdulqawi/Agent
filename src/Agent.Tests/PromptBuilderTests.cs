using Agent.Core;
using Agent.Core.Models;
using FluentAssertions;

namespace Agent.Tests;

public class PromptBuilderTests
{
    [Fact]
    public void BuildFullPrompt_WithGoalOnly_ReturnsGoal()
    {
        var goal = "Implement a REST API";
        var extraInfo = new ExtraInfo();

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Be("Goal: Implement a REST API");
    }

    [Fact]
    public void BuildFullPrompt_WithAllExtraInfo_IncludesAllContext()
    {
        var goal = "Build a web application";
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/src/myapp",
            TechStack = "Angular, .NET 8",
            Constraints = "Must use TypeScript",
            RelevantFiles = "app.component.ts, service.ts",
            Notes = "Focus on performance"
        };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("Goal: Build a web application");
        result.Should().Contain("Context (use this to understand the task):");
        result.Should().Contain("- Project path: /src/myapp");
        result.Should().Contain("- Tech stack: Angular, .NET 8");
        result.Should().Contain("- Constraints: Must use TypeScript");
        result.Should().Contain("- Relevant files: app.component.ts, service.ts");
        result.Should().Contain("- Notes: Focus on performance");
    }

    [Fact]
    public void BuildFullPrompt_WithPartialExtraInfo_IncludesOnlyProvidedContext()
    {
        var goal = "Fix the bug";
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/src/app",
            Notes = "Bug is in the login flow"
        };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("Goal: Fix the bug");
        result.Should().Contain("- Project path: /src/app");
        result.Should().Contain("- Notes: Bug is in the login flow");
        result.Should().NotContain("- Tech stack:");
        result.Should().NotContain("- Constraints:");
        result.Should().NotContain("- Relevant files:");
    }

    [Fact]
    public void SystemPrompt_ContainsDoneInstruction()
    {
        PromptBuilder.SystemPrompt.Should().Contain("<DONE>");
    }

    [Fact]
    public void SystemPrompt_ContainsAgentDescription()
    {
        PromptBuilder.SystemPrompt.Should().Contain("autonomous coding agent");
    }
}
