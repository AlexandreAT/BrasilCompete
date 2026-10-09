using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

public sealed class LiquipediaOptions
{
    public const string SectionName = "Sources:Liquipedia";

    public bool Enabled { get; set; } = true;

    public List<LiquipediaTournamentOptions> Tournaments { get; set; } = [];

    public SourceHttpOptions Http { get; set; } = new();
}
