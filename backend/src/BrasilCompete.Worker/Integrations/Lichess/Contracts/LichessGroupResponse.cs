namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>Um evento dividido em várias transmissões (por exemplo, as faixas de mesas de uma Olimpíada).</summary>
public sealed record LichessGroupResponse
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<LichessGroupTourResponse> Tours { get; init; } = [];
}
