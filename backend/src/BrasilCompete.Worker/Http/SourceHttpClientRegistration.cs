using System.Net;

using BrasilCompete.Worker.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using Polly;

namespace BrasilCompete.Worker.Http;

public static class SourceHttpClientRegistration
{
    /// <summary>
    /// Registra o cliente HTTP nomeado de uma fonte. Ordem dos handlers, de fora para dentro:
    /// cache em disco, espaçamento entre requisições, resiliência (retry com <c>Retry-After</c>) e métricas.
    /// </summary>
    public static IServiceCollection AddSourceHttpClient(
        this IServiceCollection services,
        string source,
        SourceHttpOptions options,
        WorkerOptions workerOptions)
    {
        var builder = services.AddHttpClient(source, client =>
        {
            if (options.BaseUrl.Length > 0)
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", workerOptions.UserAgent);
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        builder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        });

        builder.AddHttpMessageHandler(provider => new ResponseCacheHandler(
            source,
            TimeSpan.FromHours(options.CacheTtlHours),
            provider.GetRequiredService<HttpResponseCache>(),
            provider.GetRequiredService<TimeProvider>()));

        builder.AddHttpMessageHandler(provider => new PoliteDelayHandler(
            provider.GetRequiredService<PolitenessRegistry>()
                .Get(source, TimeSpan.FromMilliseconds(options.MinIntervalMilliseconds))));

        builder.AddStandardResilienceHandler(resilience => ConfigureResilience(resilience, options));

        builder.AddHttpMessageHandler(provider => new RequestMetricsHandler(
            source,
            provider.GetRequiredService<RequestMetrics>()));

        return services;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions resilience, SourceHttpOptions options)
    {
        var fallbackFor429 = TimeSpan.FromSeconds(options.RetryAfter429Seconds);

        resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(90);
        resilience.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
        resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(5);
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.Retry.BackoffType = DelayBackoffType.Exponential;
        resilience.Retry.Delay = TimeSpan.FromSeconds(2);
        resilience.Retry.UseJitter = true;
        resilience.Retry.DelayGenerator = arguments =>
            ValueTask.FromResult(RetryDelays.Get(arguments.Outcome.Result, fallbackFor429, DateTimeOffset.UtcNow));
    }
}
