namespace BrasilCompete.Worker.Identity.Wikidata.Contracts;

/// <summary>Resposta JSON do Wikidata Query Service (formato SPARQL 1.1 Query Results JSON).</summary>
public sealed record SparqlResponse
{
    public SparqlResults Results { get; init; } = new();
}
