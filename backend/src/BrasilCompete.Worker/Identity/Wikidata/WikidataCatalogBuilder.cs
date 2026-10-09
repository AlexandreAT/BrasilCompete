using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>
/// Gera o catálogo de identidade: para cada modalidade, três consultas pequenas (uma por critério brasileiro)
/// e depois os detalhes em lotes. Uma consulta que falha vira aviso e não interrompe as demais.
/// </summary>
public sealed class WikidataCatalogBuilder(
    WikidataClient client,
    IOptions<WikidataOptions> options,
    RequestMetrics metrics,
    TimeProvider timeProvider,
    ILogger<WikidataCatalogBuilder> logger)
{
    public async Task<IdentityCatalog> BuildAsync(CancellationToken cancellationToken)
    {
        var summaries = new List<IdentityProfileSummary>();
        var records = new List<IdentityRecord>();

        foreach (var profile in WikidataSportProfiles.All)
        {
            var (summary, profileRecords) = await BuildProfileAsync(profile, cancellationToken);
            summaries.Add(summary);
            records.AddRange(profileRecords);
            logger.LogInformation("Wikidata {Profile}: {Count} atletas ligados ao Brasil", profile.Key, profileRecords.Count);
        }

        return new IdentityCatalog
        {
            GeneratedAtUtc = timeProvider.GetUtcNow(),
            Profiles = summaries,
            Records = records,
        };
    }

    private async Task<(IdentityProfileSummary Summary, List<IdentityRecord> Records)> BuildProfileAsync(
        WikidataSportProfile profile,
        CancellationToken cancellationToken)
    {
        var requestsBefore = metrics.Get(WikidataOptions.ClientName).Requests;
        var warnings = new List<string>();
        var counts = new Dictionary<BrazilianCriterion, int>();
        var itemIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var criterion in Enum.GetValues<BrazilianCriterion>())
        {
            try
            {
                var response = await client.QueryAsync(WikidataQueryBuilder.BuildCandidates(profile, criterion), cancellationToken);
                var ids = WikidataBindingMapper.ReadItemIds(response);
                counts[criterion] = ids.Count;
                itemIds.UnionWith(ids);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                warnings.Add($"Consulta '{criterion}' falhou: {exception.Message}");
                logger.LogWarning("Wikidata {Profile}/{Criterion}: {Message}", profile.Key, criterion, exception.Message);
            }
        }

        var records = new List<IdentityRecord>();

        foreach (var batch in itemIds.Order(StringComparer.Ordinal).Chunk(options.Value.DetailsBatchSize))
        {
            try
            {
                var response = await client.QueryAsync(WikidataQueryBuilder.BuildDetails(profile, batch), cancellationToken);
                records.AddRange(response.Results.Bindings.Select(binding => WikidataBindingMapper.ToRecord(profile, binding)));
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                warnings.Add($"Detalhes de {batch.Length} itens falharam: {exception.Message}");
                logger.LogWarning("Wikidata {Profile}: detalhes falharam: {Message}", profile.Key, exception.Message);
            }
        }

        var requests = metrics.Get(WikidataOptions.ClientName).Requests - requestsBefore;

        return (new IdentityProfileSummary(profile.Key, counts, records.Count, requests, warnings), records);
    }
}
