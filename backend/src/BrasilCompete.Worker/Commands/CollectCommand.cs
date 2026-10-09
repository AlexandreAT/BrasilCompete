using System.Diagnostics;
using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.History;
using BrasilCompete.Worker.Http;
using BrasilCompete.Worker.Integrations;
using BrasilCompete.Worker.Output;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>
/// Coleta uma janela de datas em todas as fontes ativas. A falha de uma fonte nunca interrompe as outras.
/// </summary>
public sealed class CollectCommand(
    IEnumerable<IEventSource> sources,
    EventPipeline pipeline,
    HistoryUpdater history,
    OutputWriter output,
    HttpResponseCache cache,
    RequestMetrics metrics,
    TimeProvider timeProvider,
    ILogger<CollectCommand> logger)
{
    public async Task<int> RunAsync(CollectCommandOptions options, CancellationToken cancellationToken)
    {
        cache.IsEnabled = !options.NoCache;

        var startedAt = timeProvider.GetUtcNow();
        var runId = BrasiliaTime.ToLocal(startedAt).ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        logger.LogInformation("Coleta {RunId}, janela {Window}", runId, options.Window);

        var collected = new List<SportEvent>();
        var summaries = new List<SourceRunSummary>();

        foreach (var source in sources)
        {
            summaries.Add(ShouldRun(source, options)
                ? await RunSourceAsync(source, options.Window, collected, cancellationToken)
                : new SourceRunSummary(source.Name, SourceStatus.Skipped, 0, 0, 0, 0, 0, []));
        }

        var result = pipeline.Run(collected, options.Window);
        var historyResult = await history.UpdateAsync(result.Events, options.Window, startedAt, cancellationToken);
        var finishedAt = timeProvider.GetUtcNow();

        var summary = new RunSummary(
            runId,
            startedAt,
            finishedAt,
            Math.Round((finishedAt - startedAt).TotalSeconds, 1),
            options.Window,
            summaries,
            CreateTotals(result),
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

    private static bool ShouldRun(IEventSource source, CollectCommandOptions options) =>
        options.Sources is null ? source.IsEnabled : options.Sources.Contains(source.Name);

    private async Task<SourceRunSummary> RunSourceAsync(
        IEventSource source,
        DateWindow window,
        List<SportEvent> collected,
        CancellationToken cancellationToken)
    {
        var before = metrics.Get(source.Name);
        var stopwatch = Stopwatch.StartNew();
        SourceStatus status;
        IReadOnlyList<string> messages;
        var count = 0;

        try
        {
            var result = await source.CollectAsync(window, cancellationToken);
            collected.AddRange(result.Events);
            count = result.Events.Count;
            status = result.Warnings.Count == 0 ? SourceStatus.Success : SourceStatus.Partial;
            messages = result.Warnings;

            foreach (var warning in result.Warnings)
            {
                logger.LogWarning("{Source}: {Warning}", source.Name, warning);
            }

            logger.LogInformation("{Source}: {Count} eventos coletados", source.Name, count);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            status = SourceStatus.Failed;
            messages = [exception.Message];
            logger.LogError(exception, "{Source}: falha na coleta", source.Name);
        }

        var after = metrics.Get(source.Name);

        return new SourceRunSummary(
            source.Name,
            status,
            count,
            after.Requests - before.Requests,
            after.RateLimited - before.RateLimited,
            after.Failures - before.Failures,
            Math.Round(stopwatch.Elapsed.TotalSeconds, 1),
            messages);
    }

    private static RunTotals CreateTotals(PipelineResult result) => new(
        result.CollectedCount,
        result.Events.Count,
        result.Events.Count(sportEvent => sportEvent.View == EventView.Main),
        result.Events.Count(sportEvent => sportEvent.View == EventView.Individuals),
        Enum.GetValues<DiscardReason>().ToDictionary(
            reason => reason,
            reason => result.Discarded.Count(item => item.Reason == reason)),
        result.MergedCount,
        result.Conflicts.Count);
}
