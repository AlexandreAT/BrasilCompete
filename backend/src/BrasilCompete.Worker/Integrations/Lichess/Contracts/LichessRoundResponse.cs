namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>Uma rodada com as partidas (pareamentos) já publicadas.</summary>
public sealed record LichessRoundResponse
{
    public LichessRoundInfoResponse Round { get; init; } = new();

    public IReadOnlyList<LichessGameResponse> Games { get; init; } = [];
}
