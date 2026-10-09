using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Lichess;
using BrasilCompete.Worker.Integrations.Lichess.Contracts;

namespace BrasilCompete.Worker.Tests.Integrations.Lichess;

/// <summary>
/// Testes com respostas reais da Lichess salvas em 09/10/2026: a 46ª Olimpíada de Xadrez (Samarcanda, setembro de 2026),
/// rodada 5 do Open, em que o Brasil enfrentou a Irlanda.
/// </summary>
public sealed class LichessMapperTests
{
    private const string OlympiadTour = "MSQXIzkK";
    private const string RoundFive = "05Y8xn7K";

    [Fact]
    public void ParseNdjson_ReadsTheOfficialBroadcasts()
    {
        var broadcasts = LichessClient.ParseNdjson(Fixtures.Read("Lichess", "broadcasts-official.ndjson"));

        Assert.Equal(100, broadcasts.Count);
        Assert.All(broadcasts, broadcast => Assert.False(string.IsNullOrEmpty(broadcast.Tour.Id)));
    }

    [Fact]
    public void GetScope_OlympiadWithManyFederations_IsInternational() =>
        Assert.Equal(CompetitionScope.International, LichessMapper.GetScope(Players()));

    [Fact]
    public void GetScope_OnlyBrazilianPlayers_IsBrazilianDomestic() =>
        Assert.Equal(
            CompetitionScope.BrazilianDomestic,
            LichessMapper.GetScope([new LichessPlayerResponse { Name = "A", Fed = "BRA" }, new LichessPlayerResponse { Name = "B", Fed = "bra" }]));

    [Fact]
    public void ToTeamMatchups_RoundFive_IsOneBrazilVersusIrelandMatchWithFourBoards()
    {
        var tour = Tour();
        var round = tour.Rounds.Single(item => item.Id == RoundFive);
        var games = Fixtures.ReadJson<LichessRoundResponse>("Lichess", $"round-{RoundFive}.json").Games;

        var events = LichessMapper.ToTeamMatchups(
            tour.Tour,
            round,
            games,
            new LichessTeamDirectory(Players()),
            player => player.Fed == "BRA",
            CompetitionScope.International,
            TestEvents.RetrievedAt).ToList();

        var match = Assert.Single(events);
        Assert.Equal(["Brazil", "Ireland"], match.Participants.Select(participant => participant.Name));
        Assert.All(match.Participants, participant => Assert.Equal(ParticipantKind.NationalTeam, participant.Kind));
        Assert.Equal("BRA", match.Participants[0].Country);
        Assert.Equal(4, match.Participants[0].Members.Count);
        Assert.Contains(match.Participants[0].Members, member => member.Name == "Supi, Luis Paulo" && member.ExternalIds["fide"] == "2106388");
        Assert.Equal("Round 5", match.Stage);
        Assert.Equal(DateTimeOffset.Parse("2026-09-20T10:15:00Z"), match.Schedule.StartUtc);
        Assert.Equal("Asia/Samarkand", match.Schedule.OriginalTimeZone);
    }

    [Fact]
    public void ToMatchup_IndividualGame_KeepsBothPlayersWithFederation()
    {
        var tour = Tour();
        var round = tour.Rounds.Single(item => item.Id == RoundFive);
        var game = Fixtures.ReadJson<LichessRoundResponse>("Lichess", $"round-{RoundFive}.json").Games
            .First(item => item.Players.Any(player => player.Name == "Supi, Luis Paulo"));

        var sportEvent = LichessMapper.ToMatchup(tour.Tour, round, game, CompetitionScope.International, TestEvents.RetrievedAt);

        Assert.Equal(["BRA", "IRL"], sportEvent.Participants.Select(participant => participant.Country));
    }

    [Fact]
    public void GetDateSpan_UsesTheTournamentDatesInBrasilia()
    {
        var tour = Tour();
        var span = LichessMapper.GetDateSpan(tour.Tour, tour.Rounds);

        Assert.Equal(new DateOnly(2026, 9, 16), span!.Value.First);
        Assert.Equal(new DateOnly(2026, 9, 27), span.Value.Last);
    }

    [Fact]
    public void TourDetails_ListsEveryBroadcastOfTheGroup()
    {
        var group = Tour().Group!;

        Assert.Equal("46th FIDE Chess Olympiad Samarkand 2026", group.Name);
        Assert.Contains(group.Tours, tour => tour.Id == "n1pPI5Q0" && tour.Name == "Open | Matches 1-12");
        Assert.Contains(group.Tours, tour => tour.Id == OlympiadTour);
    }

    [Fact]
    public void CompetitionName_RemovesTheTableRange() =>
        Assert.Equal("46th FIDE Chess Olympiad Samarkand 2026 | Open", LichessMapper.CompetitionName(Tour().Tour));

    private static LichessTourDetailsResponse Tour() =>
        Fixtures.ReadJson<LichessTourDetailsResponse>("Lichess", $"tour-{OlympiadTour}.json");

    private static List<LichessPlayerResponse> Players() =>
        Fixtures.ReadJson<List<LichessPlayerResponse>>("Lichess", $"players-{OlympiadTour}.json");
}
