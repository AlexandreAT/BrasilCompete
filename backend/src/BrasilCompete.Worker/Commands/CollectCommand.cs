using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.History;
using BrasilCompete.Worker.Http;
using BrasilCompete.Worker.Integrations;
using BrasilCompete.Worker.Output;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>Coleta uma janela de datas em todas as fontes ativas e grava as saídas e o histórico.</summary>
public sealed class CollectCommand(
    SourceRunner runner,
    EventPipeline pipeline,
    HistoryUpdater history,
    OutputWriter output,
    HttpResponseCache cache,
    TimeProvider timeProvider,
    ILogger<CollectCommand> logger)
{
    public async Task<int> RunAsync(CollectCommandOptions options, CancellationToken cancellationToken)
    {
        cache.IsEnabled = !options.NoCache;

        var startedAt = timeProvider.GetUtcNow();
        var runId = BrasiliaTime.ToLocal(startedAt).ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        logger.LogInformation("Coleta {RunId}, janela {Window}", runId, options.Window);

        var (collected, summaries) = await runner.RunAsync(options.Window, options.Sources, cancellationToken);
        var result = pipeline.Run(collected, options.Window);
        var completeSources = summaries.Where(item => item.Status == SourceStatus.Success).Select(item => item.Source).ToHashSet(StringComparer.Ordinal);
        var historyResult = await history.UpdateAsync(result.Events, options.Window, startedAt, completeSources, cancellationToken);
        var finishedAt = timeProvider.GetUtcNow();

        var summary = new RunSummary(
            runId,
            startedAt,
            finishedAt,
            Math.Round((finishedAt - startedAt).TotalSeconds, 1),
            options.Window,
            summaries,
            RunTotalsFactory.Create(result),
            historyResult);

        var directory = await output.WriteAsync(summary, result, cancellationToken);

        logger.LogInformation(
            "Concluída em {Duration}s: {Accepted} eventos ({Main} no feed principal, {Individuals} em Indivíduos), {Discarded} descartados, {Merged} juntados, {Conflicts} conflitos",
            summary.DurationSeconds,
            summary.Totals.Accepted,
            summary.Totals.Main,
            summary.Totals.Individuals,
            result.Discarded.Count,
            summary.Totals.Merged,
            summary.Totals.Conflicts);
        logger.LogInformation("Saídas em {Directory}", directory);

        return 0;
    }
}
