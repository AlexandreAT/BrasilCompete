using System.Text.Json;

using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;
using BrasilCompete.Worker.Serialization;

namespace BrasilCompete.Worker.Output;

/// <summary>
/// Grava as saídas em <c>backend/output/runs/&lt;execução&gt;/</c> e copia para <c>backend/output/latest/</c>.
/// </summary>
public sealed class OutputWriter(BackendPaths paths)
{
    public async Task<string> WriteAsync(RunSummary summary, PipelineResult result, CancellationToken cancellationToken)
    {
        var runDirectory = Path.Combine(paths.OutputDirectory, "runs", summary.RunId);
        Directory.CreateDirectory(runDirectory);

        var events = new EventsFile(summary.RunId, summary.FinishedAtUtc, summary.Window, result.Events, result.Conflicts, result.Merges);

        await WriteJsonAsync(Path.Combine(runDirectory, "events.json"), events, cancellationToken);
        await WriteJsonAsync(Path.Combine(runDirectory, "discarded.json"), result.Discarded, cancellationToken);
        await WriteJsonAsync(Path.Combine(runDirectory, "run-summary.json"), summary, cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(runDirectory, "agenda.md"),
            AgendaMarkdownBuilder.Build(result, summary.Window, summary.FinishedAtUtc),
            cancellationToken);

        CopyToLatest(runDirectory);

        return runDirectory;
    }

    private void CopyToLatest(string runDirectory)
    {
        var latest = Path.Combine(paths.OutputDirectory, "latest");
        Directory.CreateDirectory(latest);

        foreach (var file in Directory.GetFiles(runDirectory))
        {
            File.Copy(file, Path.Combine(latest, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, JsonDefaults.Options, cancellationToken);
    }
}
