using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Identity.Wikidata;

public sealed class WikidataOptions
{
    public const string SectionName = "Sources:Wikidata";

    public const string ClientName = "wikidata";

    public bool Enabled { get; set; } = true;

    /// <summary>Quantos itens vão em cada consulta de detalhes.</summary>
    public int DetailsBatchSize { get; set; } = 200;

    public SourceHttpOptions Http { get; set; } = new();
}
