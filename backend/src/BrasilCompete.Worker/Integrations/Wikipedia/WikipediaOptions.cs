using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

public sealed class WikipediaOptions
{
    public const string SectionName = "Sources:Wikipedia";

    public bool Enabled { get; set; } = true;

    public List<WikipediaPageOptions> FootballPages { get; set; } = [];

    /// <summary>Chaves de simples de torneios de tênis, com o período de cada torneio.</summary>
    public List<WikipediaPageOptions> TennisDraws { get; set; } = [];

    public SourceHttpOptions Http { get; set; } = new();
}
