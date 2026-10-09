using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Pipeline;

public sealed record DeduplicationResult(
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<MergeConflict> Conflicts,
    IReadOnlyList<MergedEvent> Merges)
{
    public int MergedCount => Merges.Sum(merge => merge.Sources.Count - 1);
}
