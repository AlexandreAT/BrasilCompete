namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessRoundInfoResponse
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>Início da rodada, em milissegundos Unix (UTC).</summary>
    public long? StartsAt { get; init; }

    /// <summary>Início desconhecido: a rodada começa quando a anterior terminar.</summary>
    public bool? StartsAfterPrevious { get; init; }

    public string? Url { get; init; }
}
