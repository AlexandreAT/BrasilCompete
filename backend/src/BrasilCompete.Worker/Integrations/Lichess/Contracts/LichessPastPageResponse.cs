namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessPastPageResponse
{
    public int CurrentPage { get; init; }

    public int? NextPage { get; init; }

    public IReadOnlyList<LichessBroadcastResponse> CurrentPageResults { get; init; } = [];
}
