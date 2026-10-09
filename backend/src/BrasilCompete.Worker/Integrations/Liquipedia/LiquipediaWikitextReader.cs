using System.Globalization;
using System.Text.RegularExpressions;

using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// Leitura do wikitext da Liquipedia. As partidas aparecem de dois jeitos: dentro da chave do torneio
/// (<c>|R1M1={{Match|...}}</c>, comum no CS2) ou em páginas próprias <c>Match:ID ...</c> (comum no VALORANT).
/// </summary>
public static partial class LiquipediaWikitextReader
{
    private static readonly HashSet<string> BracketTemplates = new(StringComparer.OrdinalIgnoreCase) { "Matchlist", "Bracket" };
    private static readonly HashSet<string> MatchTemplates = new(StringComparer.OrdinalIgnoreCase) { "MatchPage", "Match" };
    private static readonly HashSet<string> Placeholders = new(StringComparer.OrdinalIgnoreCase) { "", "TBD", "TBA", "BYE" };

    /// <summary>Fases do torneio pelo id da chave: <c>CHAMP26GrA</c> → "Group A Matches".</summary>
    public static IReadOnlyDictionary<string, string> ReadStages(string tournamentWikitext)
    {
        var stages = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var template in Brackets(tournamentWikitext))
        {
            if (template.Named("id") is { Length: > 0 } id)
            {
                stages.TryAdd(id, template.Named("title") ?? template.Named("matchsection") ?? template.Name.Trim());
            }
        }

        return stages;
    }

    /// <summary>Partidas preenchidas dentro das chaves do próprio torneio.</summary>
    public static IReadOnlyList<LiquipediaMatch> ReadInlineMatches(string tournamentPage, string tournamentWikitext)
    {
        var matches = new List<LiquipediaMatch>();

        foreach (var bracket in Brackets(tournamentWikitext))
        {
            if (bracket.Named("id") is not { Length: > 0 } id)
            {
                continue;
            }

            foreach (var argument in bracket.Arguments)
            {
                var separator = argument.IndexOf('=', StringComparison.Ordinal);

                if (separator <= 0)
                {
                    continue;
                }

                var match = WikitextReader.ReadTemplates(argument[(separator + 1)..])
                    .FirstOrDefault(template => MatchTemplates.Contains(template.Name.Trim()));

                if (match is not null && (match.Named("opponent1") is not null || match.Named("date") is not null))
                {
                    matches.Add(ToMatch($"{tournamentPage}#{id}_{argument[..separator].Trim()}", id, match));
                }
            }
        }

        return matches;
    }

    /// <summary>Partida de uma página <c>Match:ID CHAMP26GrB 0002</c>.</summary>
    public static LiquipediaMatch? ReadMatch(string pageTitle, string content)
    {
        var template = WikitextReader.ReadTemplates(content).FirstOrDefault(item => MatchTemplates.Contains(item.Name.Trim()));

        return template is null ? null : ToMatch(pageTitle, BracketIdOf(pageTitle), template);
    }

    /// <summary>País do time pelo infobox (<c>|location=Brazil</c>).</summary>
    public static string? ReadTeamLocation(string teamWikitext)
    {
        var infobox = WikitextReader.ReadTemplates(teamWikitext)
            .FirstOrDefault(template => template.Name.Trim().StartsWith("Infobox team", StringComparison.OrdinalIgnoreCase));

        return infobox?.Named("location") is { Length: > 0 } location ? WikitextReader.ToPlainText(location) : null;
    }

    /// <summary>Uma linha <c>{{TeamPage|nome}}</c> por time, para o <c>expandtemplates</c>.</summary>
    public static string BuildTeamPageText(IEnumerable<string> teamNames) =>
        string.Join('\n', teamNames.Select(name => $"{{{{TeamPage|{name}}}}}"));

    /// <summary>
    /// Lê a expansão de <see cref="BuildTeamPageText"/>, linha a linha. Um nome sem predefinição vira um aviso em HTML
    /// (<c>&lt;div class="error"&gt;</c>) e fica de fora. Se o número de linhas não bater, nada é usado.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ParseTeamPages(IReadOnlyList<string> teamNames, string expandedWikitext)
    {
        var lines = expandedWikitext.Split('\n');
        var titles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (lines.Length != teamNames.Count)
        {
            return titles;
        }

        for (var index = 0; index < teamNames.Count; index++)
        {
            var title = lines[index].Trim();

            if (title.Length > 0 && !title.Contains('<', StringComparison.Ordinal))
            {
                titles.TryAdd(teamNames[index], title);
            }
        }

        return titles;
    }

    public static string BracketIdOf(string pageTitle)
    {
        var match = MatchTitlePattern().Match(pageTitle);

        return match.Success ? match.Groups["bracket"].Value : pageTitle;
    }

    private static IEnumerable<WikitextTemplate> Brackets(string wikitext) =>
        WikitextReader.ReadAllTemplates(wikitext).Where(template => BracketTemplates.Contains(template.Name.Trim()));

    private static LiquipediaMatch ToMatch(string key, string bracketId, WikitextTemplate template)
    {
        var (date, time, timezone) = ReadDate(template.Named("date"));

        return new LiquipediaMatch(
            key,
            bracketId,
            date,
            time,
            timezone,
            ReadTeam(template.Named("opponent1")),
            ReadTeam(template.Named("opponent2")));
    }

    private static (DateOnly? Date, TimeOnly? Time, string? Timezone) ReadDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, null, null);
        }

        var timezone = WikitextReader.ReadTemplates(value)
            .Select(template => template.Name.Trim())
            .FirstOrDefault(name => name.StartsWith("Abbr/", StringComparison.OrdinalIgnoreCase))?[5..];
        var match = DatePattern().Match(WikitextReader.ToPlainText(value));

        if (!match.Success
            || !DateOnly.TryParseExact($"{match.Groups["month"].Value} {match.Groups["day"].Value} {match.Groups["year"].Value}", "MMMM d yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return (null, null, null);
        }

        TimeOnly? time = match.Groups["time"].Success && TimeOnly.TryParseExact(match.Groups["time"].Value, ["H:mm", "HH:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;

        return (date, time, time is null ? null : timezone);
    }

    private static string? ReadTeam(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var opponent = WikitextReader.ReadTemplates(value)
            .FirstOrDefault(template => template.Name.Trim().Equals("TeamOpponent", StringComparison.OrdinalIgnoreCase));
        var name = opponent?.Argument(0) ?? opponent?.Named("template");

        return name is null || Placeholders.Contains(name.Trim()) ? null : name.Trim();
    }

    [GeneratedRegex(@"(?<month>[A-Z][a-z]+)\s+(?<day>\d{1,2}),\s*(?<year>\d{4})(\s*-\s*(?<time>\d{1,2}:\d{2}))?")]
    private static partial Regex DatePattern();

    [GeneratedRegex(@"^Match:ID[ _](?<bracket>\S+)[ _]\S+$")]
    private static partial Regex MatchTitlePattern();
}
