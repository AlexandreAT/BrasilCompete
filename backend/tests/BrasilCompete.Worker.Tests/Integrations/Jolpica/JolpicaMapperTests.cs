using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Jolpica;
using BrasilCompete.Worker.Integrations.Jolpica.Contracts;

namespace BrasilCompete.Worker.Tests.Integrations.Jolpica;

/// <summary>Testes com as respostas reais da Jolpica salvas em 09/10/2026 (temporada 2026).</summary>
public sealed class JolpicaMapperTests
{
    private static readonly DateWindow SaoPauloWeekend = new(new DateOnly(2026, 11, 6), new DateOnly(2026, 11, 8));

    private static readonly JolpicaSession[] CompetitiveSessions =
    [
        JolpicaSession.Race,
        JolpicaSession.Qualifying,
        JolpicaSession.Sprint,
        JolpicaSession.SprintQualifying,
    ];

    [Fact]
    public void ToEvents_BrazilianGrandPrix_HasRaceAndQualifyingForEachDriverOfTheGrid()
    {
        var events = Map(CompetitiveSessions);

        Assert.Equal(2 * Grid().Count, events.Count);
        Assert.All(events, sportEvent => Assert.Equal("Brazilian Grand Prix", sportEvent.Competition));
    }

    [Fact]
    public void ToEvents_Bortoleto_HasTeamWikipediaLinkAndTimeInUtc()
    {
        var race = Map(CompetitiveSessions).Single(sportEvent =>
            sportEvent.Stage == "Corrida" && sportEvent.Participants[0].Name == "Gabriel Bortoleto");

        var driver = race.Participants[0];
        Assert.Equal("BRA", driver.Country);
        Assert.Equal("Audi", driver.Team);
        Assert.Equal("Gabriel Bortoleto", driver.ExternalIds[ExternalIdKeys.EnglishWikipedia]);
        Assert.Equal(DateTimeOffset.Parse("2026-11-08T17:00:00Z"), race.Schedule.StartUtc);
        Assert.Equal(new DateOnly(2026, 11, 8), race.Schedule.Date);
        Assert.Equal(SchedulePrecision.DateAndTime, race.Schedule.Precision);
        Assert.Equal("https://api.jolpi.ca/ergast/f1/2026/20/", race.Sources[0].Url);
    }

    [Fact]
    public void ToEvents_WithPracticeSessions_TriplesTheVolumeOfANormalWeekend()
    {
        var withPractice = Map([.. CompetitiveSessions, JolpicaSession.FirstPractice, JolpicaSession.SecondPractice, JolpicaSession.ThirdPractice]);

        Assert.Equal(5 * Grid().Count, withPractice.Count);
    }

    [Fact]
    public void ToEvents_ExternalIdsAreUniquePerEvent()
    {
        var events = Map(CompetitiveSessions);

        Assert.Equal(events.Count, events.Select(sportEvent => sportEvent.Sources[0].ExternalId).Distinct().Count());
    }

    private static IReadOnlyList<SportEvent> Map(IReadOnlyCollection<JolpicaSession> sessions) =>
        JolpicaMapper.ToEvents(Races(), Grid(), sessions, SaoPauloWeekend, TestEvents.RetrievedAt);

    private static IReadOnlyList<JolpicaRaceResponse> Races() =>
        Fixtures.ReadJson<JolpicaResponse>("Jolpica", "races-2026.json").Data.RaceTable!.Races;

    private static IReadOnlyList<JolpicaDriverStandingResponse> Grid() =>
        Fixtures.ReadJson<JolpicaResponse>("Jolpica", "driverstandings-2026.json").Data.StandingsTable!.StandingsLists[^1].DriverStandings;
}
