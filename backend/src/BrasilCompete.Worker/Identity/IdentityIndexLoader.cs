using System.Text.Json;

using BrasilCompete.Worker.Serialization;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Identity;

public static class IdentityIndexLoader
{
    /// <summary>
    /// Carrega o catálogo gerado pelo comando <c>identity</c>. Sem catálogo, a camada do Wikidata fica vazia
    /// e só valem a curadoria e a nacionalidade informada pelas fontes.
    /// </summary>
    public static IdentityIndex Load(IdentityCatalogStore store, ILogger logger)
    {
        if (!File.Exists(store.FilePath))
        {
            logger.LogWarning("Catálogo de identidade não encontrado. Rode o comando 'identity' para gerar a camada do Wikidata.");

            return IdentityIndex.Empty;
        }

        using var stream = File.OpenRead(store.FilePath);
        var catalog = JsonSerializer.Deserialize<IdentityCatalog>(stream, JsonDefaults.Options) ?? new IdentityCatalog();

        return new IdentityIndex(catalog.Records);
    }
}
