using Agent.Core;
using Agent.Core.Models;
using FluentAssertions;

namespace Agent.Tests;

public class PromptBuilderTests
{
    [Fact]
    public void SystemPrompt_ContainsRequiredInstructions()
    {
        PromptBuilder.SystemPrompt.Should().Contain("autonomous coding agent");
        PromptBuilder.SystemPrompt.Should().Contain("<DONE>");
    }

    [Fact]
    public void BuildFullPrompt_WithGoalOnly_ReturnsGoal()
    {
        var goal = "Implement a login feature";
        var extraInfo = new ExtraInfo();

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().StartWith("Goal: Implement a login feature");
        result.Should().NotContain("Context");
    }

    [Fact]
    public void BuildFullPrompt_WithProjectPath_IncludesContext()
    {
        var goal = "Fix the bug";
        var extraInfo = new ExtraInfo { ProjectPath = "/src/app" };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("Goal: Fix the bug");
        result.Should().Contain("Context (use this to understand the task):");
        result.Should().Contain("- Project path: /src/app");
    }

    [Fact]
    public void BuildFullPrompt_WithTechStack_IncludesTechStack()
    {
        var goal = "Create API endpoint";
        var extraInfo = new ExtraInfo { TechStack = "ASP.NET Core, Entity Framework" };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("- Tech stack: ASP.NET Core, Entity Framework");
    }

    [Fact]
    public void BuildFullPrompt_WithConstraints_IncludesConstraints()
    {
        var goal = "Optimize performance";
        var extraInfo = new ExtraInfo { Constraints = "Must maintain backward compatibility" };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("- Constraints: Must maintain backward compatibility");
    }

    [Fact]
    public void BuildFullPrompt_WithRelevantFiles_IncludesFiles()
    {
        var goal = "Update component";
        var extraInfo = new ExtraInfo { RelevantFiles = "src/App.tsx, src/components/Header.tsx" };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("- Relevant files: src/App.tsx, src/components/Header.tsx");
    }

    [Fact]
    public void BuildFullPrompt_WithNotes_IncludesNotes()
    {
        var goal = "Refactor code";
        var extraInfo = new ExtraInfo { Notes = "Focus on readability" };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("- Notes: Focus on readability");
    }

    [Fact]
    public void BuildFullPrompt_WithAllExtraInfo_IncludesAll()
    {
        var goal = "Complete feature";
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/workspace",
            TechStack = "React, Node.js",
            Constraints = "No breaking changes",
            RelevantFiles = "index.js",
            Notes = "Important notes here"
        };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("Goal: Complete feature");
        result.Should().Contain("Context (use this to understand the task):");
        result.Should().Contain("- Project path: /workspace");
        result.Should().Contain("- Tech stack: React, Node.js");
        result.Should().Contain("- Constraints: No breaking changes");
        result.Should().Contain("- Relevant files: index.js");
        result.Should().Contain("- Notes: Important notes here");
    }

    [Fact]
    public void BuildFullPrompt_WithEmptyStrings_OmitsThem()
    {
        var goal = "Test goal";
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "",
            TechStack = null,
            Constraints = "   ",
            RelevantFiles = null,
            Notes = "Only notes"
        };

        var result = PromptBuilder.BuildFullPrompt(goal, extraInfo);

        result.Should().Contain("Goal: Test goal");
        result.Should().Contain("- Notes: Only notes");
        result.Should().NotContain("- Project path:");
        result.Should().NotContain("- Tech stack:");
        result.Should().NotContain("- Constraints:");
        result.Should().NotContain("- Relevant files:");
    }
}
