using System.Globalization;
using System.Text;

using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Validation;

/// <summary>
/// Métricas da seção 11 do plano que dependem do gabarito: cobertura, exatidão de data e de horário e perda de
/// precisão, por modalidade e por visão, mais a lista das linhas não encontradas para conferência.
/// </summary>
public static class ValidationReportBuilder
{
    public static string Build(string referenceName, string eventsRunId, IReadOnlyList<ReferenceMatch> matches)
    {
        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"# Validação — {referenceName}");
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Eventos da execução `{eventsRunId}`. Linhas do gabarito com data: {matches.Count}.");
        builder.AppendLine();

        AppendTable(builder, "Por modalidade", matches.GroupBy(match => match.Row.Sport));
        AppendTable(builder, "Por visão", matches.GroupBy(match => match.Row.View));
        AppendTable(builder, "Total", matches.GroupBy(_ => "total"));

        builder.AppendLine("## Não encontrados");
        builder.AppendLine();

        foreach (var match in matches.Where(match => !match.Found))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"- linha {match.Row.Line}: {match.Row.Sport} — {match.Row.Competition} — {match.Row.Stage} — {string.Join(" x ", match.Row.Participants)} — {match.Row.Date:yyyy-MM-dd} {match.Row.Time:HH\\:mm}");
        }

        builder.AppendLine();
        builder.AppendLine("## Data ou horário divergentes");
        builder.AppendLine();

        foreach (var match in matches.Where(match => match.DateCorrect == false || match.TimeCorrect == false))
        {
            var found = match.Event!.Schedule;
            var foundTime = found.StartUtc is { } start ? BrasiliaTime.ToLocal(start).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : found.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            builder.AppendLine(CultureInfo.InvariantCulture, $"- linha {match.Row.Line}: gabarito {match.Row.Date:yyyy-MM-dd} {match.Row.Time:HH\\:mm}; gerado {foundTime} — {string.Join(" x ", match.Row.Participants)}");
        }

        builder.AppendLine();
        builder.AppendLine("## Horário oficial perdido (evento gerado com menos precisão)");
        builder.AppendLine();

        foreach (var match in matches.Where(match => match.LostTime))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"- linha {match.Row.Line}: {match.Row.Sport} — {string.Join(" x ", match.Row.Participants)} — gerado como {match.Event!.Schedule.Precision}");
        }

        return builder.ToString();
    }

    private static void AppendTable(StringBuilder builder, string title, IEnumerable<IGrouping<string, ReferenceMatch>> groups)
    {
        builder.AppendLine(CultureInfo.InvariantCulture, $"## {title}");
        builder.AppendLine();
        builder.AppendLine("| Grupo | Gabarito | Encontrados | Cobertura | Data correta | Horário correto | Horário perdido |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var group in groups.OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var total = group.Count();
            var found = group.Count(match => match.Found);
            var dated = group.Count(match => match.DateCorrect is not null);
            var dateCorrect = group.Count(match => match.DateCorrect == true);
            var timed = group.Count(match => match.TimeCorrect is not null);
            var timeCorrect = group.Count(match => match.TimeCorrect == true);
            var lostTime = group.Count(match => match.LostTime);

            builder.AppendLine(CultureInfo.InvariantCulture, $"| {group.Key} | {total} | {found} | {Percent(found, total)} | {dateCorrect}/{dated} ({Percent(dateCorrect, dated)}) | {timeCorrect}/{timed} ({Percent(timeCorrect, timed)}) | {lostTime} |");
        }

        builder.AppendLine();
    }

    private static string Percent(int part, int total) =>
        total == 0 ? "—" : (100.0 * part / total).ToString("0", CultureInfo.InvariantCulture) + "%";
}
