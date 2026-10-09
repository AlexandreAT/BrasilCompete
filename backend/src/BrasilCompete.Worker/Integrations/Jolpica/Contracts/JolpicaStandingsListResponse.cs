namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaStandingsListResponse
{
    public string Round { get; init; } = string.Empty;

    public IReadOnlyList<JolpicaDriverStandingResponse> DriverStandings { get; init; } = [];
}
