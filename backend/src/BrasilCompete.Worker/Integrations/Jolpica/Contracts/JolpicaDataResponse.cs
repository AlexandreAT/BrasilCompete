namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaDataResponse
{
    public string Total { get; init; } = "0";

    public JolpicaRaceTableResponse? RaceTable { get; init; }

    public JolpicaDriverTableResponse? DriverTable { get; init; }

    public JolpicaStandingsTableResponse? StandingsTable { get; init; }
}
