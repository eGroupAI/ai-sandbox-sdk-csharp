namespace EGroupAI.AiSandbox.Sdk;

/// <summary>Transient HTTP retries (429 / 5xx) are limited to GET/HEAD to avoid duplicate side effects on writes.</summary>
public static class HttpRetryPolicy
{
    public static bool ShouldRetryTransientHttpStatus(string method, int statusCode)
    {
        if (statusCode != 429 && (statusCode < 500 || statusCode > 599))
            return false;
        return string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase);
    }

    public static TimeSpan GetRetryDelay(int attempt)
    {
        var safeAttempt = Math.Max(1, attempt);
        var delayMs = 200.0 * Math.Pow(2, safeAttempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(2000.0, delayMs));
    }
}
