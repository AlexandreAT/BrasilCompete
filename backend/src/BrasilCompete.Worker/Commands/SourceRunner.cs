using System.Diagnostics;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Http;
using BrasilCompete.Worker.Integrations;
using BrasilCompete.Worker.Output;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>
/// Roda cada fonte e mede status, requisições, 429 e duração. A falha de uma fonte nunca interrompe as outras.
/// </summary>
public sealed class SourceRunner(
    IEnumerable<IEventSource> sources,
    RequestMetrics metrics,
    ILogger<SourceRunner> logger)
{
    public async Task<(IReadOnlyList<SportEvent> Events, IReadOnlyList<SourceRunSummary> Summaries)> RunAsync(
        DateWindow window,
        IReadOnlySet<string>? selectedSources,
        CancellationToken cancellationToken)
    {
        var collected = new List<SportEvent>();
        var summaries = new List<SourceRunSummary>();

        foreach (var source in sources)
        {
            var shouldRun = selectedSources is null ? source.IsEnabled : selectedSources.Contains(source.Name);

            summaries.Add(shouldRun
                ? await RunSourceAsync(source, window, collected, cancellationToken)
                : new SourceRunSummary(source.Name, SourceStatus.Skipped, 0, 0, 0, 0, 0, []));
        }

        return (collected, summaries);
    }

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
            messages = [.. result.Warnings, .. result.Notes ?? []];

            foreach (var warning in result.Warnings)
            {
                logger.LogWarning("{Source}: {Warning}", source.Name, warning);
            }

            foreach (var note in result.Notes ?? [])
            {
                logger.LogInformation("{Source}: {Note}", source.Name, note);
            }

            logger.LogInformation("{Source}: {Count} eventos coletados", source.Name, count);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            status = SourceStatus.Failed;
            messages = [exception.Message];
            logger.LogError("{Source}: falha na coleta: {Message}", source.Name, exception.Message);
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
}
