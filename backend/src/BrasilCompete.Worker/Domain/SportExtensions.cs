namespace BrasilCompete.Worker.Domain;

public static class SportExtensions
{
    /// <summary>
    /// Modalidades em que o representante é o atleta (ou a dupla), e não uma equipe.
    /// A F1 entra aqui porque o piloto é o representante (plano, seção 4.3).
    /// </summary>
    public static bool IsIndividual(this Sport sport) => sport switch
    {
        Sport.Formula1 or Sport.Chess or Sport.Mma or Sport.Tennis or Sport.TableTennis or Sport.BeachVolleyball => true,
        _ => false,
    };

    public static string ToSlug(this Sport sport) => sport switch
    {
        Sport.Football => "football",
        Sport.Formula1 => "formula-1",
        Sport.Chess => "chess",
        Sport.Mma => "mma",
        Sport.EsportsValorant => "esports-valorant",
        Sport.EsportsCounterStrike => "esports-cs2",
        Sport.EsportsLeagueOfLegends => "esports-lol",
        Sport.Basketball => "basketball",
        Sport.Volleyball => "volleyball",
        Sport.BeachVolleyball => "beach-volleyball",
        Sport.Tennis => "tennis",
        Sport.TableTennis => "table-tennis",
        _ => throw new ArgumentOutOfRangeException(nameof(sport), sport, null),
    };
}
