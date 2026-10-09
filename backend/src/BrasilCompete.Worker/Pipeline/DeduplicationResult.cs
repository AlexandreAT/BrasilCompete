using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Pipeline;

public sealed record DeduplicationResult(
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<MergeConflict> Conflicts,
    int MergedCount);
