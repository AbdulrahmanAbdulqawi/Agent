using Agent.Core.Models;
using FluentAssertions;

namespace Agent.Tests;

public class ExtraInfoTests
{
    [Fact]
    public void IsEmpty_WhenAllFieldsNull_ReturnsTrue()
    {
        var extraInfo = new ExtraInfo();
        extraInfo.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenAllFieldsEmpty_ReturnsTrue()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "",
            TechStack = "",
            Constraints = "",
            RelevantFiles = "",
            Notes = ""
        };
        extraInfo.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenAllFieldsWhitespace_ReturnsTrue()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "   ",
            TechStack = "\t",
            Constraints = "\n",
            RelevantFiles = "  \n  ",
            Notes = "\r\n"
        };
        extraInfo.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenProjectPathSet_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo { ProjectPath = "/src" };
        extraInfo.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WhenTechStackSet_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo { TechStack = "C#" };
        extraInfo.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WhenConstraintsSet_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo { Constraints = "No deps" };
        extraInfo.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WhenRelevantFilesSet_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo { RelevantFiles = "file.cs" };
        extraInfo.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WhenNotesSet_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo { Notes = "Important" };
        extraInfo.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void ToContextString_WhenEmpty_ReturnsEmptyString()
    {
        var extraInfo = new ExtraInfo();
        extraInfo.ToContextString().Should().BeEmpty();
    }

    [Fact]
    public void ToContextString_WithAllFields_ReturnsFormattedString()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/app",
            TechStack = "Angular",
            Constraints = "PWA required",
            RelevantFiles = "main.ts",
            Notes = "Check tests"
        };

        var result = extraInfo.ToContextString();

        result.Should().Contain("- Project path: /app");
        result.Should().Contain("- Tech stack: Angular");
        result.Should().Contain("- Constraints: PWA required");
        result.Should().Contain("- Relevant files: main.ts");
        result.Should().Contain("- Notes: Check tests");
    }

    [Fact]
    public void ToContextString_PreservesOrder()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "1",
            TechStack = "2",
            Constraints = "3",
            RelevantFiles = "4",
            Notes = "5"
        };

        var result = extraInfo.ToContextString();
        var lines = result.Split('\n');

        lines[0].Should().Contain("Project path");
        lines[1].Should().Contain("Tech stack");
        lines[2].Should().Contain("Constraints");
        lines[3].Should().Contain("Relevant files");
        lines[4].Should().Contain("Notes");
    }
}
