using Agent.Core;
using FluentAssertions;

namespace Agent.Tests;

public class CompletionDetectionTests
{
    [Theory]
    [InlineData("<DONE>", true)]
    [InlineData("[DONE]", true)]
    [InlineData("[COMPLETE]", true)]
    [InlineData("<done>", true)]
    [InlineData("[done]", true)]
    [InlineData("[complete]", true)]
    [InlineData("Task completed successfully. <DONE>", true)]
    [InlineData("I have finished all the work.\n<DONE>", true)]
    [InlineData("The implementation is now [COMPLETE]", true)]
    public void IsComplete_WithCompletionSignal_ReturnsTrue(string text, bool expected)
    {
        var result = CompletionDetection.IsComplete(text);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Working on the task...")]
    [InlineData("Still processing the request")]
    [InlineData("Done with step 1, moving to step 2")]
    [InlineData("The task is not done yet")]
    public void IsComplete_WithoutCompletionSignal_ReturnsFalse(string? text)
    {
        var result = CompletionDetection.IsComplete(text);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WithWhitespaceAroundSignal_ReturnsTrue()
    {
        var result = CompletionDetection.IsComplete("  <DONE>  ");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithFewerThanThresholdMessages_ReturnsFalse()
    {
        var messages = new List<string> { "message1", "message1" };
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithIdenticalMessages_ReturnsTrue()
    {
        var messages = new List<string>
        {
            "I'm working on this task",
            "I'm working on this task",
            "I'm working on this task"
        };
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithDifferentMessages_ReturnsFalse()
    {
        var messages = new List<string>
        {
            "First step completed",
            "Second step in progress",
            "Third step starting now"
        };
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithSimilarLengthMessages_ReturnsTrue()
    {
        var messages = new List<string>
        {
            "Processing the request now",
            "Processing the request now!",
            "Processing the request now."
        };
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithCustomThreshold_UsesThreshold()
    {
        var messages = new List<string>
        {
            "same message",
            "same message",
            "same message",
            "same message",
            "same message"
        };
        var result = CompletionDetection.IsDoomLoop(messages, threshold: 5);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDoomLoop_WithEmptyList_ReturnsFalse()
    {
        var messages = new List<string>();
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDoomLoop_WithMixedRecentMessages_ReturnsFalse()
    {
        var messages = new List<string>
        {
            "same message",
            "same message",
            "different message now",
            "same message",
            "same message"
        };
        var result = CompletionDetection.IsDoomLoop(messages);
        result.Should().BeFalse();
    }
}
