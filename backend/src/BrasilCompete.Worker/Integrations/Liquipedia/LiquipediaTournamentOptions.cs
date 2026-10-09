using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>Um torneio do catálogo de competições-alvo de eSports (plano, seção 9.6).</summary>
public sealed class LiquipediaTournamentOptions
{
    /// <summary>Wiki do jogo: <c>valorant</c>, <c>counterstrike</c> ou <c>leagueoflegends</c>.</summary>
    public string Wiki { get; set; } = string.Empty;

    /// <summary>Página do torneio, como <c>VCT/2026/Champions</c>.</summary>
    public string Page { get; set; } = string.Empty;

    public Sport Sport { get; set; }

    public string Competition { get; set; } = string.Empty;

    /// <summary>Ligas só com organizações brasileiras (CBLOL) são <c>BrazilianDomestic</c> e não entram.</summary>
    public CompetitionScope Scope { get; set; } = CompetitionScope.International;
}
