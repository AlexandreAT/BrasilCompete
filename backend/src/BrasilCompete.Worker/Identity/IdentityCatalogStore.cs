using System.Text.Json;

using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Serialization;

namespace BrasilCompete.Worker.Identity;

/// <summary>Catálogo local, fora do Git, em <c>backend/.state/identity/wikidata-catalog.json</c>.</summary>
public sealed class IdentityCatalogStore(BackendPaths paths)
{
    public string FilePath => Path.Combine(paths.StateDirectory, "identity", "wikidata-catalog.json");

    public async Task<IdentityCatalog?> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(FilePath);

        return await JsonSerializer.DeserializeAsync<IdentityCatalog>(stream, JsonDefaults.Options, cancellationToken);
    }

    public async Task SaveAsync(IdentityCatalog catalog, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, catalog, JsonDefaults.Options, cancellationToken);
    }
}
