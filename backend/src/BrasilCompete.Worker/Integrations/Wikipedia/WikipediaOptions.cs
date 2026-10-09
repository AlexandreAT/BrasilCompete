using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

public sealed class WikipediaOptions
{
    public const string SectionName = "Sources:Wikipedia";

    public bool Enabled { get; set; } = true;

    public List<WikipediaPageOptions> FootballPages { get; set; } = [];

    /// <summary>
    /// Chaves no formato do tênis (tênis e tênis de mesa), com a modalidade e o período de cada torneio.
    /// </summary>
    public List<WikipediaPageOptions> TennisDraws { get; set; } = [];

    public SourceHttpOptions Http { get; set; } = new();
}
