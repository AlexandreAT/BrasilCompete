namespace BrasilCompete.Worker.Identity.Wikidata.Contracts;

public sealed record SparqlResults
{
    public IReadOnlyList<Dictionary<string, SparqlValue>> Bindings { get; init; } = [];
}
