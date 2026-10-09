using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Lichess.Contracts;
using BrasilCompete.Worker.Normalization;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Lichess;

/// <summary>
/// Xadrez pelas transmissões oficiais da Lichess. Só os torneios com brasileiros (federação BRA ou ID FIDE no
/// catálogo do Wikidata) têm as rodadas consultadas, para economizar requisições.
/// </summary>
public sealed class LichessEventSource(
    LichessClient client,
    IdentityIndex identityIndex,
    IOptions<LichessOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "lichess";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var tours = await FindToursAsync(window, BrasiliaTime.ToDate(retrievedAt), cancellationToken);
        var events = new List<SportEvent>();
        int withoutPlayers = 0, withBrazilians = 0, international = 0;

        foreach (var tour in tours)
        {
            var players = await client.GetPlayersAsync(tour.Tour.Id, cancellationToken);

            if (players.Count == 0)
            {
                withoutPlayers++;
                continue;
            }

            var brazilians = players.Where(IsBrazilian).ToList();

            if (brazilians.Count == 0)
            {
                continue;
            }

            withBrazilians++;
            var scope = LichessMapper.GetScope(players);
            international += scope == CompetitionScope.International ? 1 : 0;

            events.AddRange(await CollectRoundsAsync(tour.Tour.Id, players, brazilians, scope, window, retrievedAt, cancellationToken));
        }

        var notes = new List<string>
        {
            $"{tours.Count} torneios na janela; {withoutPlayers} ainda sem jogadores publicados; {withBrazilians} com brasileiros ({international} internacionais).",
        };

        return new SourceCollection(events, [], notes);
    }

    private async Task<List<LichessBroadcastResponse>> FindToursAsync(DateWindow window, DateOnly today, CancellationToken cancellationToken)
    {
        var broadcasts = new List<LichessBroadcastResponse>(await client.GetOfficialBroadcastsAsync(cancellationToken));

        if (window.From < today)
        {
            for (var page = 1; page <= options.Value.MaxPastPages; page++)
            {
                var top = await client.GetTopPageAsync(page, cancellationToken);
                var past = top.Past?.CurrentPageResults ?? [];
                broadcasts.AddRange(past);

                var oldest = past.Select(LichessMapper.GetDateSpan).OfType<(DateOnly First, DateOnly Last)>().Select(span => span.Last).DefaultIfEmpty(DateOnly.MinValue).Min();

                if (top.Past?.NextPage is null || oldest < window.From)
                {
                    break;
                }
            }
        }

        var selected = broadcasts
            .DistinctBy(broadcast => broadcast.Tour.Id)
            .Where(broadcast => LichessMapper.GetDateSpan(broadcast) is { } span && span.First <= window.To && span.Last >= window.From)
            .ToList();

        return await ExpandGroupsAsync(selected, cancellationToken);
    }

    /// <summary>
    /// As listas mostram uma transmissão por grupo (como uma faixa de mesas da Olimpíada). O detalhe de uma
    /// delas traz todas as do grupo, que entram com as mesmas datas.
    /// </summary>
    private async Task<List<LichessBroadcastResponse>> ExpandGroupsAsync(
        List<LichessBroadcastResponse> selected,
        CancellationToken cancellationToken)
    {
        var expanded = new List<LichessBroadcastResponse>(selected);
        var known = selected.Select(broadcast => broadcast.Tour.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var group in selected.Where(broadcast => broadcast.Group is not null).GroupBy(broadcast => broadcast.Group))
        {
            var representative = group.First();
            var details = await client.GetTourAsync(representative.Tour.Id, cancellationToken);

            foreach (var member in details.Group?.Tours ?? [])
            {
                if (known.Add(member.Id))
                {
                    expanded.Add(new LichessBroadcastResponse
                    {
                        Tour = representative.Tour with { Id = member.Id, Name = $"{group.Key} | {member.Name}" },
                        Group = group.Key,
                    });
                }
            }
        }

        return expanded;
    }

    private async Task<List<SportEvent>> CollectRoundsAsync(
        string tourId,
        IReadOnlyList<LichessPlayerResponse> players,
        IReadOnlyList<LichessPlayerResponse> brazilians,
        CompetitionScope scope,
        DateWindow window,
        DateTimeOffset retrievedAt,
        CancellationToken cancellationToken)
    {
        var tour = await client.GetTourAsync(tourId, cancellationToken);
        var teams = new LichessTeamDirectory(players);
        var events = new List<SportEvent>();

        foreach (var round in tour.Rounds.Where(round => round.StartsAt is { } startsAt && IsInside(window, startsAt)))
        {
            var details = await client.GetRoundAsync(round.Id, cancellationToken);

            if (details.Games.Count == 0)
            {
                events.AddRange(brazilians.Select(player => LichessMapper.ToRoundParticipation(tour.Tour, round, player, scope, retrievedAt)));
                continue;
            }

            events.AddRange(teams.HasTeams
                ? LichessMapper.ToTeamMatchups(tour.Tour, round, details.Games, teams, IsBrazilian, scope, retrievedAt)
                : details.Games
                    .Where(game => game.Players.Any(IsBrazilian))
                    .Select(game => LichessMapper.ToMatchup(tour.Tour, round, game, scope, retrievedAt)));
        }

        return events;
    }

    private bool IsBrazilian(LichessPlayerResponse player)
    {
        if (string.Equals(player.Fed, CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return identityIndex.Find(LichessMapper.ToParticipant(player), Sport.Chess) is { Confidence: Confidence.High } match
            && (match.Record.RepresentsBrazil || match.Record.BornInBrazil);
    }

    private static bool IsInside(DateWindow window, long startsAt)
    {
        var date = BrasiliaTime.ToDate(LichessMapper.ToInstant(startsAt));

        return date >= window.From && date <= window.To;
    }
}
