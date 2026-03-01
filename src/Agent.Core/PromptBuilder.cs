using Agent.Core.Models;

namespace Agent.Core;

public static class PromptBuilder
{
    public const string SystemPrompt = """
        You are an autonomous coding agent. Work on the task step by step.
        When you have fully completed the task, end your final response with exactly: <DONE>
        Do not use <DONE> until the task is truly complete. If you need to do more work, continue without it.
        """;

    public static string BuildFullPrompt(string goal, ExtraInfo extraInfo)
    {
        var parts = new List<string> { $"Goal: {goal}" };
        var context = extraInfo.ToContextString();
        if (!string.IsNullOrEmpty(context))
        {
            parts.Add("");
            parts.Add("Context (use this to understand the task):");
            parts.Add(context);
        }
        return string.Join("\n", parts);
    }
}
