using System.Text;

namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>
/// Monta as consultas SPARQL. Cada consulta parte do critério brasileiro, que é seletivo; começar pela modalidade
/// estoura o tempo em esportes grandes (basquete e futebol).
/// </summary>
public static class WikidataQueryBuilder
{
    public static string BuildCandidates(WikidataSportProfile profile, BrazilianCriterion criterion)
    {
        var practice = $"{{ ?p wdt:P106 wd:{profile.OccupationItem} }} UNION {{ ?p wdt:P641 wd:{profile.SportItem} }}";
        var brazil = criterion switch
        {
            BrazilianCriterion.RepresentsBrazil => $"?p wdt:P1532 wd:{WikidataSportProfiles.Brazil} .\n  {practice}",
            BrazilianCriterion.Citizenship => $"?p wdt:P27 wd:{WikidataSportProfiles.Brazil} .\n  {practice}",
            _ => $"{practice}\n  ?p wdt:P19 ?birthPlace .\n  ?birthPlace wdt:P17 wd:{WikidataSportProfiles.Brazil} .",
        };

        var builder = new StringBuilder();
        builder.AppendLine("SELECT DISTINCT ?p WHERE {");
        builder.AppendLine($"  {brazil}");
        builder.AppendLine($"  ?p wdt:P31 wd:{WikidataSportProfiles.Human} .");

        if (profile.MinBirthYear is { } year)
        {
            builder.AppendLine("  ?p wdt:P569 ?birth .");
            builder.AppendLine($"  FILTER(YEAR(?birth) >= {year})");
            builder.AppendLine("  FILTER NOT EXISTS { ?p wdt:P570 ?death }");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>Detalhes de um lote de itens, agregados por item para não multiplicar linhas.</summary>
    public static string BuildDetails(WikidataSportProfile profile, IEnumerable<string> itemIds)
    {
        var builder = new StringBuilder();
        builder.Append("SELECT ?p (SAMPLE(?labelEn) AS ?nameEn) (SAMPLE(?labelPt) AS ?namePt) (SAMPLE(?article) AS ?enwiki) ");
        builder.Append("(SAMPLE(?birth) AS ?birthDate) ");
        builder.Append("(GROUP_CONCAT(DISTINCT ?forSport; separator=\"|\") AS ?forSports) ");
        builder.Append("(GROUP_CONCAT(DISTINCT ?birthCountry; separator=\"|\") AS ?birthCountries) ");
        builder.Append("(GROUP_CONCAT(DISTINCT ?citizenship; separator=\"|\") AS ?citizenships) ");

        foreach (var key in profile.ExternalIdProperties.Keys)
        {
            builder.Append($"(GROUP_CONCAT(DISTINCT ?{Variable(key)}; separator=\"|\") AS ?ext_{Variable(key)}) ");
        }

        builder.AppendLine("WHERE {");
        builder.AppendLine($"  VALUES ?p {{ {string.Join(' ', itemIds.Select(id => $"wd:{id}"))} }}");
        builder.AppendLine("  OPTIONAL { ?p rdfs:label ?labelEn FILTER(LANG(?labelEn) = \"en\") }");
        builder.AppendLine("  OPTIONAL { ?p rdfs:label ?labelPt FILTER(LANG(?labelPt) IN (\"pt-br\", \"pt\")) }");
        builder.AppendLine("  OPTIONAL { ?enwikiPage schema:about ?p ; schema:isPartOf <https://en.wikipedia.org/> ; schema:name ?article }");
        builder.AppendLine("  OPTIONAL { ?p wdt:P569 ?birth }");
        builder.AppendLine("  OPTIONAL { ?p wdt:P1532 ?forSport }");
        builder.AppendLine("  OPTIONAL { ?p wdt:P19/wdt:P17 ?birthCountry }");
        builder.AppendLine("  OPTIONAL { ?p wdt:P27 ?citizenship }");

        foreach (var (key, property) in profile.ExternalIdProperties)
        {
            builder.AppendLine($"  OPTIONAL {{ ?p wdt:{property} ?{Variable(key)} }}");
        }

        builder.AppendLine("}");
        builder.AppendLine("GROUP BY ?p");

        return builder.ToString();
    }

    public static string Variable(string externalIdKey) => externalIdKey.Replace('-', '_');
}
