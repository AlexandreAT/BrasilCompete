namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaRaceTableResponse
{
    public IReadOnlyList<JolpicaRaceResponse> Races { get; init; } = [];
}
