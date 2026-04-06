using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EGroupAI.AiSandbox.Sdk;

public sealed class AiSandboxClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly int _maxRetries;

    public AiSandboxClient(string baseUrl, string apiKey, HttpClient? httpClient = null, int maxRetries = 2, int timeoutSeconds = 30)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _maxRetries = maxRetries;
        _http = httpClient ?? new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    private async Task<JsonDocument> JsonAsync(string method, string path, object? body = null)
    {
        using var response = await SendAsync(method, path, body, "application/json");
        var raw = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(raw);
    }

    private async Task<HttpResponseMessage> SendAsync(string method, string path, object? body, string accept)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), $"{_baseUrl}/api/v1{path}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            if (body is not null)
            {
                var payload = JsonSerializer.Serialize(body);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request, accept == "text/event-stream"
                    ? HttpCompletionOption.ResponseHeadersRead
                    : HttpCompletionOption.ResponseContentRead);
            }
            catch (HttpRequestException) when (attempt < _maxRetries)
            {
                await Task.Delay(HttpRetryPolicy.GetRetryDelay(attempt + 1));
                continue;
            }
            catch (TaskCanceledException) when (attempt < _maxRetries)
            {
                await Task.Delay(HttpRetryPolicy.GetRetryDelay(attempt + 1));
                continue;
            }

            if (HttpRetryPolicy.ShouldRetryTransientHttpStatus(method, (int)response.StatusCode) && attempt < _maxRetries)
            {
                response.Dispose();
                await Task.Delay(HttpRetryPolicy.GetRetryDelay(attempt + 1));
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();
                string? trace = null;
                if (response.Headers.TryGetValues("x-trace-id", out var values))
                    trace = values.FirstOrDefault();
                response.Dispose();
                throw new ApiException((int)response.StatusCode, raw, trace);
            }

            return response;
        }
    }

    public Task<JsonDocument> CreateAgentAsync(object payload) => JsonAsync("POST", "/agents", payload);
    public Task<JsonDocument> UpdateAgentAsync(int agentId, object payload) => JsonAsync("PUT", $"/agents/{agentId}", payload);
    public Task<JsonDocument> ListAgentsAsync(string query = "") => JsonAsync("GET", $"/agents{(string.IsNullOrWhiteSpace(query) ? "" : "?" + query)}");
    public Task<JsonDocument> GetAgentDetailAsync(int agentId) => JsonAsync("GET", $"/agents/{agentId}");
    public Task<JsonDocument> CreateChatChannelAsync(int agentId, object payload) => JsonAsync("POST", $"/agents/{agentId}/channels", payload);
    public Task<JsonDocument> SendChatAsync(int agentId, object payload) => JsonAsync("POST", $"/agents/{agentId}/chat", payload);
    public Task<JsonDocument> GetChatHistoryAsync(int agentId, string channelId, string query = "limit=50&page=0") => JsonAsync("GET", $"/agents/{agentId}/channels/{channelId}/messages?{query}");
    public Task<JsonDocument> GetKnowledgeBaseArticlesAsync(int agentId, int collectionId, string query = "startIndex=0") => JsonAsync("GET", $"/agents/{agentId}/collections/{collectionId}/articles?{query}");
    public Task<JsonDocument> CreateKnowledgeBaseAsync(int agentId, object payload) => JsonAsync("POST", $"/agents/{agentId}/collections", payload);
    public Task<JsonDocument> UpdateKnowledgeBaseStatusAsync(int agentCollectionId, object payload) => JsonAsync("PATCH", $"/agent-collections/{agentCollectionId}/status", payload);
    public Task<JsonDocument> ListKnowledgeBasesAsync(int agentId, string query = "activeOnly=false") => JsonAsync("GET", $"/agents/{agentId}/collections?{query}");

    public async IAsyncEnumerable<string> SendChatStreamAsync(int agentId, object payload)
    {
        using var response = await SendAsync("POST", $"/agents/{agentId}/chat", payload, "text/event-stream");
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
            var data = line[6..].Trim();
            if (data == "[DONE]") yield break;
            yield return data;
        }
    }
}
