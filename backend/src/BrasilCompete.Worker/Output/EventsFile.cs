using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

namespace BrasilCompete.Worker.Output;

/// <summary>Conteúdo do <c>events.json</c>: os eventos consolidados da janela, com conflitos e junções.</summary>
public sealed record EventsFile(
    string RunId,
    DateTimeOffset GeneratedAtUtc,
    DateWindow Window,
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<MergeConflict> Conflicts,
    IReadOnlyList<MergedEvent> Merges);
