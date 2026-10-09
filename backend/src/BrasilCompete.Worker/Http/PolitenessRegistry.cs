using System.Collections.Concurrent;

namespace BrasilCompete.Worker.Http;

/// <summary>
/// Um <see cref="PolitenessGate"/> por fonte, compartilhado entre os handlers que o HttpClientFactory recria.
/// </summary>
public sealed class PolitenessRegistry(TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<string, PolitenessGate> gates = new();

    public PolitenessGate Get(string source, TimeSpan minInterval) =>
        gates.GetOrAdd(source, _ => new PolitenessGate(minInterval, timeProvider));
}
