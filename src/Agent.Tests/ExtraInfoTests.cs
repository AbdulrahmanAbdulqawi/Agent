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
    public void IsEmpty_WhenAllFieldsWhitespace_ReturnsTrue()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "   ",
            TechStack = "",
            Constraints = null,
            RelevantFiles = "  ",
            Notes = ""
        };
        extraInfo.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenAnyFieldHasValue_ReturnsFalse()
    {
        var extraInfo = new ExtraInfo
        {
            ProjectPath = "/src/app"
        };
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
            ProjectPath = "/src/app",
            TechStack = "React, Node.js",
            Constraints = "No external libraries",
            RelevantFiles = "index.ts",
            Notes = "Use latest patterns"
        };

        var result = extraInfo.ToContextString();

        result.Should().Contain("- Project path: /src/app");
        result.Should().Contain("- Tech stack: React, Node.js");
        result.Should().Contain("- Constraints: No external libraries");
        result.Should().Contain("- Relevant files: index.ts");
        result.Should().Contain("- Notes: Use latest patterns");
    }

    [Fact]
    public void ToContextString_WithPartialFields_OnlyIncludesNonEmpty()
    {
        var extraInfo = new ExtraInfo
        {
            TechStack = "Python",
            Notes = "Focus on readability"
        };

        var result = extraInfo.ToContextString();

        result.Should().Contain("- Tech stack: Python");
        result.Should().Contain("- Notes: Focus on readability");
        result.Should().NotContain("- Project path:");
        result.Should().NotContain("- Constraints:");
        result.Should().NotContain("- Relevant files:");
    }
}
