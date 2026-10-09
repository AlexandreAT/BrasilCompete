namespace BrasilCompete.Worker.Domain;

/// <summary>
/// Alcance da competição, decidido pelo adapter a partir do catálogo de competições da fonte.
/// </summary>
public enum CompetitionScope
{
    /// <summary>Participantes de mais de um país (Libertadores, F1, VCT Americas).</summary>
    International,

    /// <summary>Liga nacional estrangeira (NBA, LaLiga): só entra pela visão de Indivíduos.</summary>
    ForeignDomestic,

    /// <summary>Liga nacional brasileira (Brasileirão, CBLOL): nunca entra.</summary>
    BrazilianDomestic,
}
