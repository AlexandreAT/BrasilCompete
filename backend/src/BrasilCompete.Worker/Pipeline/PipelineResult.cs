using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Pipeline;

public sealed record PipelineResult(
    int CollectedCount,
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<DiscardedEvent> Discarded,
    IReadOnlyList<MergeConflict> Conflicts,
    int MergedCount);
