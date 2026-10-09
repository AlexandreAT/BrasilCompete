namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaExpandResultResponse
{
    public string? Wikitext { get; init; }
}
