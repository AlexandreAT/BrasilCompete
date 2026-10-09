using System.Text.RegularExpressions;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Lichess.Contracts;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Lichess;

public static partial class LichessMapper
{
    public const string License = "Dados da API da Lichess (fatos do torneio)";

    public const string FideIdKey = "fide";

    public static DateTimeOffset ToInstant(long unixMilliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds);

    /// <summary>Datas do torneio no horário de Brasília, pelas datas do torneio ou, na falta delas, pelas rodadas.</summary>
    public static (DateOnly First, DateOnly Last)? GetDateSpan(LichessBroadcastResponse broadcast) =>
        GetDateSpan(broadcast.Tour, broadcast.Rounds.Append(broadcast.Round).OfType<LichessRoundInfoResponse>());

    public static (DateOnly First, DateOnly Last)? GetDateSpan(LichessTourResponse tour, IEnumerable<LichessRoundInfoResponse> rounds)
    {
        var instants = tour.Dates.Count > 0
            ? tour.Dates
            : rounds.Select(round => round.StartsAt).OfType<long>().ToList();

        if (instants.Count == 0)
        {
            return null;
        }

        return (BrasiliaTime.ToDate(ToInstant(instants.Min())), BrasiliaTime.ToDate(ToInstant(instants.Max())));
    }

    /// <summary>Nome da competição sem a faixa de mesas da transmissão ("| Matches 13-37").</summary>
    public static string CompetitionName(LichessTourResponse tour) =>
        TableRangePattern().Replace(tour.Name, string.Empty).Trim();

    /// <summary>O torneio é internacional quando há duas ou mais federações (plano, seção 3.2).</summary>
    public static CompetitionScope GetScope(IEnumerable<LichessPlayerResponse> players)
    {
        var federations = players
            .Select(player => player.Fed?.Trim().ToUpperInvariant())
            .Where(federation => !string.IsNullOrEmpty(federation))
            .Distinct()
            .ToList();

        if (federations.Count >= 2)
        {
            return CompetitionScope.International;
        }

        return federations is [CountryCodes.Brazil] ? CompetitionScope.BrazilianDomestic : CompetitionScope.ForeignDomestic;
    }

    public static Participant ToParticipant(LichessPlayerResponse player)
    {
        var externalIds = new Dictionary<string, string>();

        if (player.FideId is { } fideId)
        {
            externalIds[FideIdKey] = fideId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return new Participant
        {
            Name = player.Name,
            Kind = ParticipantKind.Athlete,
            Country = string.IsNullOrWhiteSpace(player.Fed) ? null : player.Fed.Trim().ToUpperInvariant(),
            ExternalIds = externalIds,
        };
    }

    public static SportEvent ToMatchup(
        LichessTourResponse tour,
        LichessRoundInfoResponse round,
        LichessGameResponse game,
        CompetitionScope scope,
        DateTimeOffset retrievedAtUtc) => new()
    {
        Sport = Sport.Chess,
        Competition = CompetitionName(tour),
        Stage = round.Name,
        Format = EventFormat.Matchup,
        Scope = scope,
        Schedule = ToSchedule(tour, round),
        Venue = tour.Info?.Location,
        Participants = game.Players.Select(ToParticipant).ToList(),
        Sources = [Source(tour, round, $"{tour.Id}/{round.Id}/{game.Id}", retrievedAtUtc)],
    };

    /// <summary>
    /// Competições por equipes: as partidas da rodada viram um confronto por par de equipes
    /// ("Brazil x Ireland"), com os jogadores de cada tabuleiro como membros.
    /// </summary>
    public static IEnumerable<SportEvent> ToTeamMatchups(
        LichessTourResponse tour,
        LichessRoundInfoResponse round,
        IReadOnlyList<LichessGameResponse> games,
        LichessTeamDirectory teams,
        Func<LichessPlayerResponse, bool> isBrazilian,
        CompetitionScope scope,
        DateTimeOffset retrievedAtUtc)
    {
        var boards = games
            .Select(game => (Game: game, Teams: game.Players.Select(teams.TeamOf).ToList()))
            .Where(board => board.Teams is [{ } first, { } second] && first != second)
            .GroupBy(board => string.Join('|', board.Teams.Order(StringComparer.Ordinal)));

        foreach (var match in boards)
        {
            var players = match.SelectMany(board => board.Game.Players).ToList();

            if (!players.Any(isBrazilian))
            {
                continue;
            }

            var order = match.First().Teams.OfType<string>().ToList();

            yield return new SportEvent
            {
                Sport = Sport.Chess,
                Competition = CompetitionName(tour),
                Stage = round.Name,
                Format = EventFormat.Matchup,
                Scope = scope,
                Schedule = ToSchedule(tour, round),
                Venue = tour.Info?.Location,
                Participants = order.Select(team => teams.ToTeamParticipant(team, players.Where(player => teams.TeamOf(player) == team))).ToList(),
                Sources = [Source(tour, round, $"{tour.Id}/{round.Id}/{match.Key}", retrievedAtUtc)],
            };
        }
    }

    /// <summary>Rodada ainda sem pareamento: o brasileiro participa, adversário a definir.</summary>
    public static SportEvent ToRoundParticipation(
        LichessTourResponse tour,
        LichessRoundInfoResponse round,
        LichessPlayerResponse player,
        CompetitionScope scope,
        DateTimeOffset retrievedAtUtc) => new()
    {
        Sport = Sport.Chess,
        Competition = CompetitionName(tour),
        Stage = round.Name,
        Format = EventFormat.Participation,
        Scope = scope,
        Schedule = ToSchedule(tour, round),
        Venue = tour.Info?.Location,
        Participants = [ToParticipant(player)],
        Sources = [Source(tour, round, $"{tour.Id}/{round.Id}/{player.FideId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? player.Name}", retrievedAtUtc)],
    };

    private static Schedule ToSchedule(LichessTourResponse tour, LichessRoundInfoResponse round) =>
        round.StartsAt is { } startsAt
            ? Schedule.AtTime(ToInstant(startsAt), tour.Info?.TimeZone ?? "Etc/UTC")
            : Schedule.ToBeConfirmed();

    private static SourceReference Source(LichessTourResponse tour, LichessRoundInfoResponse round, string externalId, DateTimeOffset retrievedAtUtc) => new()
    {
        Source = LichessEventSource.SourceName,
        Url = round.Url ?? tour.Url ?? $"https://lichess.org/broadcast/-/{tour.Id}",
        License = License,
        RetrievedAtUtc = retrievedAtUtc,
        ExternalId = externalId,
    };

    [GeneratedRegex(@"\s*\|\s*Matches\s+[\d+\-]+\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex TableRangePattern();
}
