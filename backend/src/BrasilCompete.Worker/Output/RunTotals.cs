using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Output;

public sealed record RunTotals(
    int Collected,
    int Accepted,
    int Main,
    int Individuals,
    IReadOnlyDictionary<DiscardReason, int> Discarded,
    int Merged,
    int Conflicts);
