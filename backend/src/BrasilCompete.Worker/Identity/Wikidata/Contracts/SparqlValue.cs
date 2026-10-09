namespace BrasilCompete.Worker.Identity.Wikidata.Contracts;

public sealed record SparqlValue
{
    public string Type { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
