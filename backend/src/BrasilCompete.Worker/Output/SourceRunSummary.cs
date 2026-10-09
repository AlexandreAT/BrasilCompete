using BrasilCompete.Worker.Integrations;

namespace BrasilCompete.Worker.Output;

public sealed record SourceRunSummary(
    string Source,
    SourceStatus Status,
    int EventsCollected,
    int Requests,
    int RateLimited,
    int Failures,
    double DurationSeconds,
    IReadOnlyList<string> Messages);
