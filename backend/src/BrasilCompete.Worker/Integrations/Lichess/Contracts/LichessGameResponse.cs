namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessGameResponse
{
    public string Id { get; init; } = string.Empty;

    /// <summary>Brancas e pretas, nessa ordem.</summary>
    public IReadOnlyList<LichessPlayerResponse> Players { get; init; } = [];
}
