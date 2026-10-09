namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaRevisionResponse
{
    public LiquipediaSlotsResponse? Slots { get; init; }
}
