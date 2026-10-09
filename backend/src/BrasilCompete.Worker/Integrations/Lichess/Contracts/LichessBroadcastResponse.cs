namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>
/// Uma transmissão. A lista oficial traz todas as rodadas (<see cref="Rounds"/>);
/// a lista paginada de transmissões passadas traz só a última (<see cref="Round"/>).
/// </summary>
public sealed record LichessBroadcastResponse
{
    public LichessTourResponse Tour { get; init; } = new();

    public IReadOnlyList<LichessRoundInfoResponse> Rounds { get; init; } = [];

    public LichessRoundInfoResponse? Round { get; init; }

    /// <summary>Nome do grupo, quando o evento tem várias transmissões.</summary>
    public string? Group { get; init; }
}
