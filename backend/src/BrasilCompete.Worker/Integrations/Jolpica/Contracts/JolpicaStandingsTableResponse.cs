namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaStandingsTableResponse
{
    public IReadOnlyList<JolpicaStandingsListResponse> StandingsLists { get; init; } = [];
}
