using EGroupAI.AiSandbox.Sdk;

var client = new AiSandboxClient(
    Environment.GetEnvironmentVariable("AI_SANDBOX_BASE_URL") ?? "https://www.egroupai.com",
    Environment.GetEnvironmentVariable("AI_SANDBOX_API_KEY") ?? string.Empty
);

var result = await client.CreateAgentAsync(new
{
    agentDisplayName = "C# SDK Quickstart",
    agentDescription = "Created by C# SDK"
});

Console.WriteLine(result.RootElement);
