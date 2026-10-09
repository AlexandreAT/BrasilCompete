using System.Globalization;
using System.Text;

using BrasilCompete.Worker.Identity.Wikidata;

namespace BrasilCompete.Worker.Identity;

/// <summary>Resumo do catálogo para o relatório: quantos atletas, por qual critério e com quais IDs externos.</summary>
public static class IdentitySummaryBuilder
{
    public static string Build(IdentityCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Catálogo de identidade (Wikidata)");
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Gerado em {catalog.GeneratedAtUtc:yyyy-MM-dd HH:mm} UTC.");
        builder.AppendLine();
        builder.AppendLine("| Modalidade | Atletas | Representa o Brasil (P1532) | Nasceu no Brasil | Entram (representa ou nasceu) | Só cidadania | Artigo na Wikipedia (en) | Requisições |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var profile in catalog.Profiles)
        {
            var records = catalog.Records.Where(record => record.ProfileKey == profile.Key).ToList();
            var included = records.Count(record => record.RepresentsBrazil || record.BornInBrazil);

            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {profile.Key} | {records.Count} | {records.Count(record => record.RepresentsBrazil)} | {records.Count(record => record.BornInBrazil)} | {included} | {records.Count(record => record.IsCitizenshipOnly)} | {records.Count(record => record.EnglishWikipediaTitle is not null)} | {profile.Requests} |");
        }

        builder.AppendLine();
        builder.AppendLine("## IDs externos");
        builder.AppendLine();
        builder.AppendLine("| Modalidade | ID | Atletas com o ID |");
        builder.AppendLine("| --- | --- | --- |");

        foreach (var profile in WikidataSportProfiles.All)
        {
            var records = catalog.Records.Where(record => record.ProfileKey == profile.Key).ToList();

            foreach (var (key, property) in profile.ExternalIdProperties)
            {
                var withId = records.Count(record => record.ExternalIds.ContainsKey(key));
                builder.AppendLine(CultureInfo.InvariantCulture, $"| {profile.Key} | {key} ({property}) | {withId} de {records.Count} |");
            }
        }

        var warnings = catalog.Profiles.SelectMany(profile => profile.Warnings.Select(warning => $"{profile.Key}: {warning}")).ToList();

        if (warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Avisos");
            builder.AppendLine();

            foreach (var warning in warnings)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"- {warning}");
            }
        }

        return builder.ToString();
    }
}
