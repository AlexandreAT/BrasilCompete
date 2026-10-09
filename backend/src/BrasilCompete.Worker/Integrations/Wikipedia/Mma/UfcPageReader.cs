using System.Globalization;
using System.Text.RegularExpressions;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>
/// Lê a "List of UFC events" (eventos agendados e passados), o card de lutas de cada evento e a
/// "List of current UFC fighters". O card não traz bandeiras: o país vem da bandeira de cada lutador no elenco
/// e, para quem não está nele, do catálogo do Wikidata, pelo artigo do lutador.
/// </summary>
public static partial class UfcPageReader
{
    private static readonly string[] DateFormats = ["MMM d, yyyy", "MMMM d, yyyy", "MMM. d, yyyy", "d MMMM yyyy"];

    private static readonly HashSet<string> BoutSeparators = new(StringComparer.OrdinalIgnoreCase) { "vs.", "vs", "def.", "def" };

    public static IReadOnlyList<UfcEventListing> ReadEventList(string html)
    {
        var document = new HtmlParser().ParseDocument(html);
        var events = new List<UfcEventListing>();

        foreach (var sectionId in new[] { "Scheduled_events", "Past_events" })
        {
            if (FindSectionTable(document, sectionId) is { } table)
            {
                events.AddRange(ReadEventTable(table));
            }
        }

        return events;
    }

    public static IReadOnlyList<UfcBout> ReadFightCard(string html)
    {
        var document = new HtmlParser().ParseDocument(html);
        var bouts = new List<UfcBout>();

        foreach (var table in document.QuerySelectorAll("table.toccolours").OfType<IHtmlTableElement>())
        {
            string? card = null;

            foreach (var row in table.Rows)
            {
                var cells = row.Cells.ToList();

                if (cells.Count == 1)
                {
                    card = Text(cells[0]);
                    continue;
                }

                if (cells.Count >= 4 && BoutSeparators.Contains(Text(cells[2])))
                {
                    bouts.Add(new UfcBout(Text(cells[0]), ReadFighter(cells[1]), ReadFighter(cells[3]), card));
                }
            }
        }

        return bouts;
    }

    /// <summary>
    /// País de cada lutador do elenco: em cada linha, a bandeira (<c>{{flagicon|BRA}}</c>) e o nome, na mesma célula
    /// ou na seguinte. Só o link dessa célula conta: os outros links da linha são do evento e do adversário da última luta.
    /// </summary>
    public static UfcRoster ReadRoster(string html)
    {
        var document = new HtmlParser().ParseDocument(html);
        var roster = new UfcRoster();

        foreach (var row in document.QuerySelectorAll("tr"))
        {
            if (row.QuerySelector("span.flagicon") is not { } flag
                || flag.Closest("td, th") is not { } flagCell
                || FlagCountry(flag) is not { } country)
            {
                continue;
            }

            var nameCell = flagCell.TextContent.Trim().Length == 0 ? flagCell.NextElementSibling : flagCell;
            var link = nameCell?.QuerySelectorAll("a[rel='mw:WikiLink']").FirstOrDefault(item => !flag.Contains(item));
            var name = link?.TextContent.Trim() ?? (nameCell is null ? null : NameNoisePattern().Replace(Text(nameCell), string.Empty).Trim());

            if (!string.IsNullOrEmpty(name))
            {
                roster.Add(name, link is null ? null : TitleOf(link), country);
            }
        }

        return roster;
    }

    /// <summary>
    /// Código do país pela predefinição da bandeira (<c>{{flagicon|BRA}}</c> ou <c>{{#invoke:flag|icon|BRA}}</c>)
    /// ou, sem ela, pelo nome do país no link da imagem.
    /// </summary>
    private static string? FlagCountry(IElement flag)
    {
        var dataMw = flag.GetAttribute("data-mw") ?? string.Empty;
        var code = CountryCodeParameterPattern().Match(dataMw);

        return code.Success
            ? code.Groups["code"].Value
            : CountryCodes.FromEnglishName(flag.QuerySelector("a[title]")?.GetAttribute("title"));
    }

    private static IHtmlTableElement? FindSectionTable(IDocument document, string sectionId)
    {
        var heading = document.GetElementById(sectionId);
        var section = heading?.Closest("section");

        return section?.QuerySelector("table") as IHtmlTableElement;
    }

    private static IEnumerable<UfcEventListing> ReadEventTable(IHtmlTableElement table)
    {
        var headers = table.Rows.FirstOrDefault()?.Cells.Select(Text).ToList() ?? [];
        var eventColumn = headers.FindIndex(header => header.StartsWith("Event", StringComparison.OrdinalIgnoreCase));
        var dateColumn = headers.FindIndex(header => header.StartsWith("Date", StringComparison.OrdinalIgnoreCase));
        var venueColumn = headers.FindIndex(header => header.StartsWith("Venue", StringComparison.OrdinalIgnoreCase));
        var locationColumn = headers.FindIndex(header => header.StartsWith("Location", StringComparison.OrdinalIgnoreCase));

        if (eventColumn < 0 || dateColumn < 0)
        {
            yield break;
        }

        foreach (var row in table.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();

            if (cells.Count <= Math.Max(eventColumn, dateColumn)
                || !DateOnly.TryParseExact(Text(cells[dateColumn]), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                continue;
            }

            var link = cells[eventColumn].QuerySelector("a[rel='mw:WikiLink']");

            yield return new UfcEventListing(
                Text(cells[eventColumn]),
                link is null ? null : TitleOf(link),
                date,
                venueColumn >= 0 && venueColumn < cells.Count ? Text(cells[venueColumn]) : null,
                locationColumn >= 0 && locationColumn < cells.Count ? Text(cells[locationColumn]) : null);
        }
    }

    private static UfcFighter ReadFighter(IElement cell)
    {
        var link = cell.QuerySelector("a[rel='mw:WikiLink']");

        return link is null
            ? new UfcFighter(Text(cell).Replace("(c)", string.Empty, StringComparison.Ordinal).Trim(), null)
            : new UfcFighter(link.TextContent.Trim(), TitleOf(link));
    }

    /// <summary>Título do artigo a partir do link do Parsoid (<c>./Charles_Oliveira</c>); links vermelhos não têm artigo.</summary>
    private static string? TitleOf(IElement link)
    {
        var href = link.GetAttribute("href");

        if (href is null || !href.StartsWith("./", StringComparison.Ordinal) || href.Contains("action=edit", StringComparison.Ordinal))
        {
            return null;
        }

        return Uri.UnescapeDataString(href[2..].Split('#', '?')[0]).Replace('_', ' ');
    }

    private static string Text(IElement element) =>
        string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();

    [GeneratedRegex(@"""wt"":""(?<code>[A-Z]{3})""")]
    private static partial Regex CountryCodeParameterPattern();

    /// <summary>Notas de rodapé ("[a]", "[143]") e a marca de campeão ("(c)") que acompanham o nome.</summary>
    [GeneratedRegex(@"\[[^\]]*\]|\(c\)")]
    private static partial Regex NameNoisePattern();
}
