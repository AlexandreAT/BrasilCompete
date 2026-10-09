using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Identity.Wikidata;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>Gera o catálogo de identidade a partir do Wikidata e o resumo em <c>output/identity/summary.md</c>.</summary>
public sealed class IdentityCommand(
    WikidataCatalogBuilder builder,
    IdentityCatalogStore store,
    BackendPaths paths,
    ILogger<IdentityCommand> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var catalog = await builder.BuildAsync(cancellationToken);
        await store.SaveAsync(catalog, cancellationToken);

        var directory = Path.Combine(paths.OutputDirectory, "identity");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, "summary.md"), IdentitySummaryBuilder.Build(catalog), cancellationToken);

        logger.LogInformation("Catálogo com {Count} atletas salvo em {Path}", catalog.Records.Count, store.FilePath);
        logger.LogInformation("Resumo em {Path}", Path.Combine(directory, "summary.md"));

        return 0;
    }
}
