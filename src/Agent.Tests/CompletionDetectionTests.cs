using Agent.Core;
using FluentAssertions;

namespace Agent.Tests;

public class CompletionDetectionTests
{
    [Theory]
    [InlineData("<DONE>", true)]
    [InlineData("[DONE]", true)]
    [InlineData("[COMPLETE]", true)]
    [InlineData("Task finished <DONE>", true)]
    [InlineData("I have completed all tasks. <DONE>", true)]
    [InlineData("  <DONE>  ", true)]
    [InlineData("<done>", true)]
    [InlineData("[complete]", true)]
    public void IsComplete_WithCompletionSignal_ReturnsTrue(string text, bool expected)
    {
        var result = CompletionDetection.IsComplete(text);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("Working on the task...")]
    [InlineData("I'm still processing")]
    [InlineData("DONE without brackets")]
    [InlineData("Almost <DONE")]
    public void IsComplete_WithoutCompletionSignal_ReturnsFalse(string? text)
    {
        var result = CompletionDetection.IsComplete(text);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithFewerThanThresholdMessages_ReturnsFalse()
    {
        var messages = new List<string> { "Hello", "Hello" };
        var result = CompletionDetection.IsDoomLoop(messages, threshold: 3);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithIdenticalMessages_ReturnsTrue()
    {
        var messages = new List<string> 
        { 
            "I'm processing the task",
            "I'm processing the task",
            "I'm processing the task"
        };
        var result = CompletionDetection.IsDoomLoop(messages, threshold: 3);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithSimilarLengthMessages_ReturnsTrue()
    {
        var messages = new List<string> 
        { 
            "Processing task now...",
            "Processing task here..",
            "Processing task done.."
        };
        var result = CompletionDetection.IsDoomLoop(messages, threshold: 3);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithDifferentMessages_ReturnsFalse()
    {
        var messages = new List<string> 
        { 
            "Step 1: Analyzing requirements",
            "Step 2: Writing code for the feature",
            "Step 3: Testing the implementation thoroughly"
        };
        var result = CompletionDetection.IsDoomLoop(messages, threshold: 3);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithEmptyList_ReturnsFalse()
    {
        var messages = new List<string>();
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeFalse();
    }
}
