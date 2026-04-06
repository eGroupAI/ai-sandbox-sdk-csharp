# Integration Guide (C#)

This SDK is designed for low-change, low-touch customer integration.

## Goals
- Stable API surface for v1.
- Explicit timeout and retry controls.
- Streaming chat support (`text/event-stream`).

## Retry safety
- **429 / 5xx** automatic retries apply only to **GET** and **HEAD**. **POST / PUT / PATCH** are not retried on those status codes to avoid duplicate side effects.
- **HttpRequestException** and **TaskCanceledException** (e.g. timeouts) may still be retried for all methods, up to `maxRetries`.

## Install
`dotnet add package EGroupAI.AiSandbox.Sdk`

## First Steps
1. Construct `AiSandboxClient` with `baseUrl` and `apiKey`.
2. Call `CreateAgentAsync(...)`.
3. Create a chat channel with `CreateChatChannelAsync(...)` and send the first message with `SendChatAsync(...)` or `SendChatStreamAsync(...)`.

## Errors
- On HTTP errors, `ApiException` exposes `TraceId` when the server sends `x-trace-id`.
