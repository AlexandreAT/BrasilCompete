using System.Text.Json;

using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Serialization;

namespace BrasilCompete.Worker.History;

/// <summary>Histórico local, fora do Git, em <c>backend/.state/history.json</c>.</summary>
public sealed class EventHistoryStore(BackendPaths paths)
{
    private string FilePath => Path.Combine(paths.StateDirectory, "history.json");

    public async Task<EventHistoryFile> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return new EventHistoryFile();
        }

        await using var stream = File.OpenRead(FilePath);

        return await JsonSerializer.DeserializeAsync<EventHistoryFile>(stream, JsonDefaults.Options, cancellationToken)
            ?? new EventHistoryFile();
    }

    public async Task SaveAsync(EventHistoryFile history, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(paths.StateDirectory);
        var temporaryPath = $"{FilePath}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, history, JsonDefaults.Options, cancellationToken);
        }

        File.Move(temporaryPath, FilePath, overwrite: true);
    }
}
