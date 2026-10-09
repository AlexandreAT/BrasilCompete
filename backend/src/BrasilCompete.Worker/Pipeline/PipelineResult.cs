using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Pipeline;

public sealed record PipelineResult(
    int CollectedCount,
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<DiscardedEvent> Discarded,
    IReadOnlyList<MergeConflict> Conflicts,
    IReadOnlyList<MergedEvent> Merges)
{
    public int MergedCount => Merges.Sum(merge => merge.Sources.Count - 1);
}
