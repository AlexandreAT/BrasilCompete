using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.History;

namespace BrasilCompete.Worker.Output;

/// <summary>Conteúdo do <c>run-summary.json</c>: status, contagens, requisições, duração e erros por fonte.</summary>
public sealed record RunSummary(
    string RunId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    double DurationSeconds,
    DateWindow Window,
    IReadOnlyList<SourceRunSummary> Sources,
    RunTotals Totals,
    HistoryUpdateResult History);
