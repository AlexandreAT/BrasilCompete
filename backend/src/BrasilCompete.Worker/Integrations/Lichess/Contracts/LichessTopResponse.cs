namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>Página de <c>/api/broadcast/top</c>: transmissões ativas e uma página de passadas.</summary>
public sealed record LichessTopResponse
{
    public IReadOnlyList<LichessBroadcastResponse> Active { get; init; } = [];

    public LichessPastPageResponse? Past { get; init; }
}
