namespace EGroupAI.AiSandbox.Sdk;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string ResponseBody { get; }
    public string? TraceId { get; }

    public ApiException(int statusCode, string responseBody)
        : this(statusCode, responseBody, null)
    {
    }

    public ApiException(int statusCode, string responseBody, string? traceId)
        : base(FormatMessage(statusCode, responseBody, traceId))
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        TraceId = traceId;
    }

    private static string FormatMessage(int statusCode, string responseBody, string? traceId) =>
        string.IsNullOrEmpty(traceId)
            ? $"HTTP {statusCode}: {responseBody}"
            : $"HTTP {statusCode}: {responseBody} (trace_id={traceId})";
}
