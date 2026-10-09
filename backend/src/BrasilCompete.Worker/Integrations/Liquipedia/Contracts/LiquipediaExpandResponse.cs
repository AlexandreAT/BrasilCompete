namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

/// <summary>Resposta de <c>api.php?action=expandtemplates&amp;prop=wikitext</c> (formatversion=2).</summary>
public sealed record LiquipediaExpandResponse
{
    public LiquipediaExpandResultResponse? Expandtemplates { get; init; }
}
