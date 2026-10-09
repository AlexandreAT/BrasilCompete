using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>
/// Itens e propriedades descobertos pela busca do Wikidata (<c>wbsearchentities</c>) em 09/10/2026,
/// como pede o plano (seção 9.5). Futebol, basquete e vôlei se limitam a atletas vivos e nascidos a partir
/// de um ano de corte, para a consulta caber no limite de 60 s do serviço.
/// </summary>
public static class WikidataSportProfiles
{
    public const string Brazil = "Q155";
    public const string Human = "Q5";

    public static IReadOnlyList<WikidataSportProfile> All { get; } =
    [
        new("formula-1", [Sport.Formula1], "Q1968", "Q10841764", null, Ids()),
        new("chess", [Sport.Chess], "Q718", "Q10873124", null, Ids(("fide", "P1440"), ("lichess", "P8976"))),
        new("mma", [Sport.Mma], "Q114466", "Q11607585", null, Ids(("ufc", "P9722"), ("sherdog", "P2818"), ("tapology", "P9728"))),
        new("tennis", [Sport.Tennis], "Q847", "Q10833314", null, Ids(("atp", "P536"), ("wta", "P597"))),
        new("table-tennis", [Sport.TableTennis], "Q3930", "Q13382519", null, Ids(("wtt", "P1364"))),
        new("volleyball", [Sport.Volleyball], "Q1734", "Q15117302", 1980, Ids(("fivb", "P2801"))),
        new("beach-volleyball", [Sport.BeachVolleyball], "Q4543", "Q17361156", null, Ids(("fivb", "P2801"))),
        new("basketball", [Sport.Basketball], "Q5372", "Q3665646", 1980, Ids(("nba", "P3647"), ("basketball-reference", "P2685"))),
        new(
            "esports",
            [Sport.EsportsValorant, Sport.EsportsCounterStrike, Sport.EsportsLeagueOfLegends],
            "Q300920",
            "Q4379701",
            null,
            Ids(("liquipedia", "P10918"), ("hltv", "P8878"))),
        new("football", [Sport.Football], "Q2736", "Q937857", 1984, Ids(("transfermarkt", "P2446"))),
    ];

    private static Dictionary<string, string> Ids(params (string Key, string Property)[] ids) =>
        ids.ToDictionary(id => id.Key, id => id.Property);
}
