using System.Net;
using System.Text;
using Xunit;

namespace EGroupAI.AiSandbox.Sdk.Tests;

public class AiSandboxClientContractTests
{
    [Fact]
    public async Task Retries_Get_On_Transient_5xx_Then_Succeeds()
    {
        var handler = new QueueHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("temporary failure", Encoding.UTF8, "text/plain")
            },
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true,\"payload\":{\"items\":[]}}", Encoding.UTF8, "application/json")
            });

        using var http = new HttpClient(handler);
        var client = new AiSandboxClient("https://api.example.test", "test-key", http, maxRetries: 2, timeoutSeconds: 3);

        using var result = await client.ListAgentsAsync();

        Assert.Equal(2, handler.Calls);
        Assert.True(result.RootElement.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task Does_Not_Retry_Post_On_Http_5xx()
    {
        var handler = new QueueHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("write failed", Encoding.UTF8, "text/plain")
            };
            response.Headers.Add("x-trace-id", "trace-post-1");
            return response;
        });

        using var http = new HttpClient(handler);
        var client = new AiSandboxClient("https://api.example.test", "test-key", http, maxRetries: 2, timeoutSeconds: 3);

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            client.SendChatAsync(123, new { channelId = "c-1", message = "hello" }));

        Assert.Equal(503, ex.StatusCode);
        Assert.Equal("trace-post-1", ex.TraceId);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Retries_Post_On_Network_Failure_Then_Succeeds()
    {
        var handler = new QueueHandler(
            _ => throw new HttpRequestException("network timeout"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true,\"payload\":{\"messageId\":\"m-1\"}}", Encoding.UTF8, "application/json")
            });

        using var http = new HttpClient(handler);
        var client = new AiSandboxClient("https://api.example.test", "test-key", http, maxRetries: 2, timeoutSeconds: 3);

        using var result = await client.SendChatAsync(123, new { channelId = "c-1", message = "hello" });

        Assert.Equal(2, handler.Calls);
        Assert.True(result.RootElement.GetProperty("ok").GetBoolean());
    }

    private sealed class QueueHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _queue;

        public int Calls { get; private set; }

        public QueueHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] handlers)
        {
            _queue = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>(handlers);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            Calls += 1;
            if (_queue.Count == 0)
                throw new InvalidOperationException("No more mocked responses.");
            var next = _queue.Dequeue();
            return Task.FromResult(next(request));
        }
    }
}
