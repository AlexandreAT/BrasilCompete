using System.Text.Json;

using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Serialization;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Manual;

/// <summary>
/// Eventos da curadoria manual, para lacunas e correções. Vence todas as fontes na deduplicação.
/// </summary>
public sealed class ManualEventSource(
    IOptions<ManualOptions> options,
    BackendPaths paths,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "manual";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var path = paths.Resolve(options.Value.FilePath);

        if (!File.Exists(path))
        {
            return new SourceCollection([], [$"Arquivo de curadoria não encontrado: {path}"]);
        }

        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<ManualEventsFile>(stream, JsonDefaults.Options, cancellationToken)
            ?? new ManualEventsFile();

        var retrievedAt = timeProvider.GetUtcNow();
        var events = file.Events.Select(entry => ManualEventMapper.ToSportEvent(entry, retrievedAt)).ToList();

        return new SourceCollection(events, []);
    }
}
