using System.Collections.Concurrent;

namespace BrasilCompete.Worker.Http;

/// <summary>
/// Conta, por fonte, as requisições que de fato saíram para a rede (respostas do cache não contam).
/// </summary>
public sealed class RequestMetrics
{
    private readonly ConcurrentDictionary<string, Counters> counters = new();

    public void Record(string source, int? statusCode)
    {
        var counter = counters.GetOrAdd(source, _ => new Counters());
        Interlocked.Increment(ref counter.Requests);

        if (statusCode == 429)
        {
            Interlocked.Increment(ref counter.RateLimited);
        }
        else if (statusCode is null or >= 400)
        {
            Interlocked.Increment(ref counter.Failures);
        }
    }

    public SourceRequestStats Get(string source) =>
        counters.TryGetValue(source, out var counter)
            ? new SourceRequestStats(counter.Requests, counter.RateLimited, counter.Failures)
            : new SourceRequestStats(0, 0, 0);

    private sealed class Counters
    {
        public int Requests;
        public int RateLimited;
        public int Failures;
    }
}
