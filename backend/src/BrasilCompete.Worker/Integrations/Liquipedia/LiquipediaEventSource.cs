using BrasilCompete.Worker.Domain;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// eSports (VALORANT, CS2 e LoL) pela API do MediaWiki da Liquipedia, para os torneios do catálogo:
/// página do torneio (fases e partidas embutidas), páginas de partida e páginas dos times (país).
/// </summary>
public sealed class LiquipediaEventSource(
    LiquipediaClient client,
    IOptions<LiquipediaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "liquipedia";

    private const string CommonsWiki = "commons";
    private const string TimezoneModule = "Module:Timezone/Data";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var timezones = await LoadTimezonesAsync(cancellationToken);
        var events = new List<SportEvent>();
        var warnings = new List<string>();
        var notes = new List<string>();

        foreach (var tournament in options.Value.Tournaments)
        {
            var pages = await client.GetPagesAsync(tournament.Wiki, [tournament.Page], cancellationToken);

            if (!pages.TryGetValue(tournament.Page, out var page))
            {
                warnings.Add($"Torneio não encontrado: {tournament.Wiki}/{tournament.Page}");
                continue;
            }

            var stages = LiquipediaWikitextReader.ReadStages(page.Content);
            var matches = await LoadMatchesAsync(tournament, page.Content, stages.Keys, cancellationToken);
            var teams = await LoadTeamsAsync(tournament.Wiki, matches, cancellationToken);

            var tournamentEvents = matches
                .Select(match => LiquipediaMapper.ToEvent(match, tournament, stages, teams, timezones, retrievedAt))
                .OfType<SportEvent>()
                .Where(sportEvent => window.Overlaps(sportEvent.Schedule))
                .ToList();

            events.AddRange(tournamentEvents);
            notes.Add($"{tournament.Competition} ({tournament.Page}): {matches.Count} partidas publicadas, {tournamentEvents.Count} na janela, {teams.Count} times ({teams.Values.Count(team => team.Location is null)} sem país no infobox).");
        }

        return new SourceCollection(events, warnings, notes);
    }

    /// <summary>
    /// Prefixos das páginas de partida (<c>ID_CHAMP26</c>). Ids com um prefixo comum viram uma consulta só.
    /// </summary>
    public static IReadOnlyList<string> MatchPagePrefixes(IEnumerable<string> bracketIds)
    {
        var ids = bracketIds.ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var common = ids.Aggregate((first, second) => new string(first.Zip(second).TakeWhile(pair => pair.First == pair.Second).Select(pair => pair.First).ToArray()));

        return common.Length >= 4 ? [$"ID_{common}"] : ids.Select(id => $"ID_{id}").ToList();
    }

    private async Task<IReadOnlyDictionary<string, TimeSpan>> LoadTimezonesAsync(CancellationToken cancellationToken)
    {
        var module = await client.GetPagesAsync(CommonsWiki, [TimezoneModule], cancellationToken);

        return module.TryGetValue(TimezoneModule, out var page)
            ? LiquipediaTimezones.Parse(page.Content)
            : new Dictionary<string, TimeSpan>();
    }

    private async Task<List<LiquipediaMatch>> LoadMatchesAsync(
        LiquipediaTournamentOptions tournament,
        string tournamentWikitext,
        IEnumerable<string> bracketIds,
        CancellationToken cancellationToken)
    {
        var matches = new List<LiquipediaMatch>(LiquipediaWikitextReader.ReadInlineMatches(tournament.Page, tournamentWikitext));
        var titles = new List<string>();

        foreach (var prefix in MatchPagePrefixes(bracketIds))
        {
            titles.AddRange(await client.ListPagesAsync(tournament.Wiki, LiquipediaClient.MatchNamespace, prefix, cancellationToken));
        }

        var pages = await client.GetPagesAsync(tournament.Wiki, titles, cancellationToken);

        matches.AddRange(pages
            .Select(pair => LiquipediaWikitextReader.ReadMatch(pair.Key, pair.Value.Content))
            .OfType<LiquipediaMatch>());

        return matches.DistinctBy(match => match.PageTitle).ToList();
    }

    private async Task<Dictionary<string, LiquipediaTeam>> LoadTeamsAsync(
        string wiki,
        IEnumerable<LiquipediaMatch> matches,
        CancellationToken cancellationToken)
    {
        var keys = matches.SelectMany(match => new[] { match.FirstTeam, match.SecondTeam }).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var teamPages = await client.GetTeamPageTitlesAsync(wiki, keys, cancellationToken);
        var titles = keys.ToDictionary(key => key, key => teamPages.GetValueOrDefault(key, key), StringComparer.OrdinalIgnoreCase);
        var pages = await client.GetPagesAsync(wiki, titles.Values, cancellationToken);

        return keys.ToDictionary(
            key => key,
            key => pages.TryGetValue(titles[key], out var page)
                ? new LiquipediaTeam(page.Title, LiquipediaWikitextReader.ReadTeamLocation(page.Content))
                : new LiquipediaTeam(titles[key], null),
            StringComparer.OrdinalIgnoreCase);
    }
}
