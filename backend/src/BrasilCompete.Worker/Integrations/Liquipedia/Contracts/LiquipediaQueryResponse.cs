namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

/// <summary>Resposta de <c>api.php?action=query</c> (formatversion=2).</summary>
public sealed record LiquipediaQueryResponse
{
    public LiquipediaQueryResultResponse? Query { get; init; }

    public LiquipediaContinueResponse? Continue { get; init; }
}
