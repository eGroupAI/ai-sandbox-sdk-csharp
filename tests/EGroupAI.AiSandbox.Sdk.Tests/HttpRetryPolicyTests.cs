using Xunit;

namespace EGroupAI.AiSandbox.Sdk.Tests;

public class HttpRetryPolicyTests
{
    [Fact]
    public void Retries_Transient_Status_Only_For_Get_Or_Head()
    {
        Assert.True(HttpRetryPolicy.ShouldRetryTransientHttpStatus("GET", 503));
        Assert.True(HttpRetryPolicy.ShouldRetryTransientHttpStatus("HEAD", 429));
        Assert.False(HttpRetryPolicy.ShouldRetryTransientHttpStatus("POST", 503));
        Assert.False(HttpRetryPolicy.ShouldRetryTransientHttpStatus("GET", 404));
    }
}
