namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessTourResponse
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>Início e fim do torneio, em milissegundos Unix (UTC).</summary>
    public IReadOnlyList<long> Dates { get; init; } = [];

    public int? Tier { get; init; }

    public string? Url { get; init; }

    public LichessTourInfoResponse? Info { get; init; }
}
