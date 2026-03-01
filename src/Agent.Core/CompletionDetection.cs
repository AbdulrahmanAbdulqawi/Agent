namespace Agent.Core;

public static class CompletionDetection
{
    private static readonly string[] CompletionSignals = ["<DONE>", "[COMPLETE]", "[DONE]"];

    public static bool IsComplete(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = text.Trim();
        return CompletionSignals.Any(signal =>
            normalized.EndsWith(signal, StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains(signal, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsDoomLoop(IReadOnlyList<string> recentMessages, int threshold = 3)
    {
        if (recentMessages.Count < threshold) return false;
        var last = recentMessages[^1];
        var count = 0;
        for (var i = recentMessages.Count - 1; i >= 0 && count < threshold; i--)
        {
            if (AreSimilar(recentMessages[i], last)) count++;
            else break;
        }
        return count >= threshold;
    }

    private static bool AreSimilar(string a, string b)
    {
        if (a == b) return true;
        var aNorm = a.Trim().Length;
        var bNorm = b.Trim().Length;
        if (aNorm == 0 || bNorm == 0) return false;
        var diff = Math.Abs(aNorm - bNorm);
        var maxLen = Math.Max(aNorm, bNorm);
        return (double)diff / maxLen < 0.1; // Within 10% length
    }
}
