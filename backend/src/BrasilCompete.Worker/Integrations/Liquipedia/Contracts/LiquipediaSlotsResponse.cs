namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaSlotsResponse
{
    public LiquipediaSlotResponse? Main { get; init; }
}
