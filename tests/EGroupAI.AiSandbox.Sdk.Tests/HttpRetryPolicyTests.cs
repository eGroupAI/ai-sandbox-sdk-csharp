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

    [Fact]
    public void Uses_Exponential_Backoff_With_Cap()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(200), HttpRetryPolicy.GetRetryDelay(1));
        Assert.Equal(TimeSpan.FromMilliseconds(400), HttpRetryPolicy.GetRetryDelay(2));
        Assert.Equal(TimeSpan.FromMilliseconds(800), HttpRetryPolicy.GetRetryDelay(3));
        Assert.Equal(TimeSpan.FromMilliseconds(1600), HttpRetryPolicy.GetRetryDelay(4));
        Assert.Equal(TimeSpan.FromMilliseconds(2000), HttpRetryPolicy.GetRetryDelay(5));
        Assert.Equal(TimeSpan.FromMilliseconds(2000), HttpRetryPolicy.GetRetryDelay(8));
    }
}
