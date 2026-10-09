using System.Net;

namespace BrasilCompete.Worker.Http;

public static class RetryDelays
{
    /// <summary>
    /// Respeita o <c>Retry-After</c> da fonte. Num 429 sem o cabeçalho, espera o tempo configurado.
    /// Nos demais casos devolve <c>null</c>, e vale o backoff exponencial padrão.
    /// </summary>
    public static TimeSpan? Get(HttpResponseMessage? response, TimeSpan fallbackFor429, DateTimeOffset now)
    {
        if (response is null)
        {
            return null;
        }

        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
        {
            return delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var wait = date - now;

            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return response.StatusCode == HttpStatusCode.TooManyRequests ? fallbackFor429 : null;
    }
}
