using System.Net;
using System.Net.Http.Headers;

using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Tests.Http;

public sealed class RetryDelaysTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Fallback = TimeSpan.FromMinutes(1);

    [Fact]
    public void Get_UsesRetryAfterInSeconds()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), RetryDelays.Get(response, Fallback, Now));
    }

    [Fact]
    public void Get_UsesRetryAfterDate()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(Now.AddSeconds(45));

        Assert.Equal(TimeSpan.FromSeconds(45), RetryDelays.Get(response, Fallback, Now));
    }

    [Fact]
    public void Get_TooManyRequestsWithoutHeader_UsesTheConfiguredWait()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

        Assert.Equal(Fallback, RetryDelays.Get(response, Fallback, Now));
    }

    [Fact]
    public void Get_OtherErrors_UseTheDefaultBackoff()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        Assert.Null(RetryDelays.Get(response, Fallback, Now));
    }
}
