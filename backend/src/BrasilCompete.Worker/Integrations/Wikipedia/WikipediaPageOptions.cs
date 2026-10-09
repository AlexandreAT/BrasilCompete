using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>
/// Uma página do catálogo de competições-alvo (plano, seção 9.6). O alcance vem da configuração,
/// e não é deduzido dos participantes.
/// </summary>
public sealed class WikipediaPageOptions
{
    /// <summary>Título do artigo na Wikipedia em inglês, com sublinhados.</summary>
    public string Title { get; set; } = string.Empty;

    public Sport Sport { get; set; } = Sport.Football;

    /// <summary>Nome da competição. Vazio: usa o campo <c>round</c> da predefinição (ex.: "Friendly").</summary>
    public string? Competition { get; set; }

    public CompetitionScope Scope { get; set; } = CompetitionScope.International;

    /// <summary>Período do torneio, para chaves de tênis (a Wikipedia não traz o dia de cada jogo).</summary>
    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }
}
