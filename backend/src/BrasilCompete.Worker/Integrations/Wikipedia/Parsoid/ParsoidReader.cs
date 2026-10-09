using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

using AngleSharp.Html.Parser;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;

/// <summary>
/// Lê as predefinições do HTML do Parsoid pelo atributo <c>data-mw</c>, que traz o nome e os parâmetros em JSON
/// (plano, seção 3.3). Percorre a página em ordem, guardando o título de seção mais próximo de cada predefinição.
/// </summary>
public static partial class ParsoidReader
{
    private static readonly HashSet<string> GenericHeadings = new(StringComparer.OrdinalIgnoreCase)
    {
        "Matches", "Match", "Summary", "Results", "Fixtures", "Results and fixtures", "Bracket", "Schedule",
    };

    public static IReadOnlyList<ParsoidTemplate> ReadTemplates(string html, Func<string, bool> nameFilter)
    {
        var document = new HtmlParser().ParseDocument(html);
        var templates = new List<ParsoidTemplate>();
        string? heading = null;
        string? topHeading = null;
        int? headingYear = null;

        foreach (var element in document.All)
        {
            if (element.LocalName is "h2" or "h3" or "h4")
            {
                var text = element.TextContent.Trim();
                headingYear = ReadYear(text) ?? headingYear;
                heading = GenericHeadings.Contains(text) ? heading : text;
                topHeading = element.LocalName == "h2" ? text : topHeading;
                continue;
            }

            if (element.GetAttribute("typeof")?.Contains("mw:Transclusion", StringComparison.Ordinal) != true
                || element.GetAttribute("data-mw") is not { } dataMw)
            {
                continue;
            }

            foreach (var (name, parameters) in ParseDataMw(dataMw))
            {
                if (nameFilter(name))
                {
                    templates.Add(new ParsoidTemplate(name, parameters, heading, headingYear, topHeading));
                }
            }
        }

        return templates;
    }

    private static IEnumerable<(string Name, IReadOnlyDictionary<string, string> Parameters)> ParseDataMw(string dataMw)
    {
        using var document = JsonDocument.Parse(dataMw);

        if (!document.RootElement.TryGetProperty("parts", out var parts))
        {
            yield break;
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object || !part.TryGetProperty("template", out var template))
            {
                continue;
            }

            var name = template.GetProperty("target").GetProperty("wt").GetString()?.Trim() ?? string.Empty;
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (template.TryGetProperty("params", out var parametersElement))
            {
                foreach (var parameter in parametersElement.EnumerateObject())
                {
                    if (parameter.Value.TryGetProperty("wt", out var value))
                    {
                        parameters[parameter.Name] = value.GetString() ?? string.Empty;
                    }
                }
            }

            yield return (name, parameters);
        }
    }

    private static int? ReadYear(string text)
    {
        var match = YearPattern().Match(text);

        return match.Success ? int.Parse(match.Value, CultureInfo.InvariantCulture) : null;
    }

    [GeneratedRegex(@"\b20\d{2}\b")]
    private static partial Regex YearPattern();
}
