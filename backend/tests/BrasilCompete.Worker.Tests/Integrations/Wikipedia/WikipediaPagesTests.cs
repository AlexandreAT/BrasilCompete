using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia;
using BrasilCompete.Worker.Integrations.Wikipedia.Football;
using BrasilCompete.Worker.Integrations.Wikipedia.Mma;
using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;

namespace BrasilCompete.Worker.Tests.Integrations.Wikipedia;

/// <summary>
/// HTML do Parsoid salvo em 09/10/2026 e recortado nas seções usadas (ver <c>Fixtures/README.md</c>).
/// </summary>
public sealed class WikipediaPagesTests
{
    private static readonly WikipediaPageOptions Libertadores = new()
    {
        Title = "2026_Copa_Libertadores_final_stages",
        Competition = "Copa Libertadores 2026",
    };

    private static readonly WikipediaPageOptions WomenTeam = new() { Title = "Brazil_women's_national_football_team" };

    [Fact]
    public void Libertadores_SecondLegWithoutLinks_IsResolvedFromTheFirstLeg()
    {
        var events = FootballEvents("2026_Copa_Libertadores_final_stages.html", Libertadores);

        var secondLeg = events.Single(sportEvent =>
            sportEvent.Schedule.Date == new DateOnly(2026, 9, 16) && sportEvent.Participants[0].Name == "Corinthians");

        var corinthians = secondLeg.Participants[0];
        Assert.Equal(ParticipantKind.Club, corinthians.Kind);
        Assert.Equal("BRA", corinthians.Country);
        Assert.Equal("SC Corinthians Paulista", corinthians.ExternalIds[ExternalIdKeys.EnglishWikipedia]);
        Assert.Equal(DateTimeOffset.Parse("2026-09-17T00:30:00Z"), secondLeg.Schedule.StartUtc);
        Assert.Equal("Quarter-finals", secondLeg.Stage);
    }

    [Fact]
    public void Libertadores_FinalWithUndefinedTeams_IsSkipped()
    {
        var events = FootballEvents("2026_Copa_Libertadores_final_stages.html", Libertadores);

        Assert.DoesNotContain(events, sportEvent => sportEvent.Participants.Any(participant => participant.Name.Contains("finalist", StringComparison.OrdinalIgnoreCase)));
        Assert.Contains(events, sportEvent => sportEvent.Stage == "Semi-finals" && sportEvent.Participants.Any(participant => participant.Name == "Palmeiras"));
    }

    [Fact]
    public void WomenTeam_DateWithoutYear_UsesTheSectionYear()
    {
        var events = FootballEvents("Brazil_womens_national_football_team.html", WomenTeam);

        var friendly = events.Single(sportEvent => sportEvent.Schedule.Date == new DateOnly(2026, 10, 10));

        Assert.Equal(["Brasil (feminino)", "Argentina (feminino)"], friendly.Participants.Select(participant => participant.Name));
        Assert.Equal("Amistoso internacional", friendly.Competition);
        Assert.Equal(DateTimeOffset.Parse("2026-10-10T19:30:00Z"), friendly.Schedule.StartUtc);
    }

    [Fact]
    public void WomenTeam_WorldCupMatchAgainstTbd_IsAParticipationWithoutTime()
    {
        var events = FootballEvents("Brazil_womens_national_football_team.html", WomenTeam);

        var worldCup = events.First(sportEvent => sportEvent.Schedule.Date == new DateOnly(2027, 6, 24));

        Assert.Equal(EventFormat.Participation, worldCup.Format);
        Assert.Equal(SchedulePrecision.DateOnly, worldCup.Schedule.Precision);
    }

    [Fact]
    public void UfcEventList_ReadsScheduledEventsWithDateAndArticle()
    {
        var events = UfcPageReader.ReadEventList(Fixtures.Read("Wikipedia", "List_of_UFC_events.html"));

        var ufc335 = events.Single(listing => listing.Name == "UFC 335: Oliveira vs. Lopes");
        Assert.Equal(new DateOnly(2026, 12, 12), ufc335.Date);
        Assert.Equal("UFC 335", ufc335.Title);
        Assert.Contains("T-Mobile Arena", ufc335.Venue);
    }

    [Fact]
    public void UfcFightCard_ReadsBoutsWithWikipediaArticles()
    {
        var bouts = UfcPageReader.ReadFightCard(Fixtures.Read("Wikipedia", "UFC_335.html"));

        var main = bouts[0];
        Assert.Equal("Lightweight", main.WeightClass);
        Assert.Equal(new UfcFighter("Charles Oliveira", "Charles Oliveira"), main.First);
        Assert.Equal(new UfcFighter("Diego Lopes", "Diego Lopes (fighter)"), main.Second);
        Assert.Contains(bouts, bout => bout.Second.Name == "Alex Pereira");
    }

    [Fact]
    public void UfcRoster_ReadsTheFlagOfEachFighter()
    {
        var roster = UfcPageReader.ReadRoster(Fixtures.Read("Wikipedia", "List_of_current_UFC_fighters.html"));

        Assert.Equal("BRA", roster.CountryOf(new UfcFighter("Rodolfo Vieira", "Rodolfo Vieira")));
        Assert.Equal("BRA", roster.CountryOf(new UfcFighter("Valesca Machado", "Valesca Machado")));
        Assert.Equal("BRA", roster.CountryOf(new UfcFighter("Joanderson Brito", "Joanderson Brito")));
        Assert.Equal("ITA", roster.CountryOf(new UfcFighter("Marvin Vettori", "Marvin Vettori")));
        Assert.Equal("GEO", roster.CountryOf(new UfcFighter("Giga Chikadze", "Giga Chikadze")));

        // Sem artigo, vale o nome da célula; os outros links da linha (evento e adversário) não contam.
        Assert.Equal("BRA", roster.CountryOf(new UfcFighter("Rodolfo Bellato", null)));
        Assert.Equal("USA", roster.CountryOf(new UfcFighter("Melissa Amaya", null)));
        Assert.Equal(7, roster.Count);
    }

    [Fact]
    public void UfcMapper_UsesTheRosterCountry()
    {
        var listing = new UfcEventListing("UFC Fight Night: Rosas Jr. vs. Barcelos", "UFC Fight Night 289", new DateOnly(2026, 9, 26), null, null);
        var bout = new UfcBout("Middleweight", new UfcFighter("Rodolfo Vieira", "Rodolfo Vieira", "BRA"), new UfcFighter("Robert Bryczek", "Robert Bryczek"), null);

        var sportEvent = UfcMapper.ToEvent(listing, bout, TestEvents.RetrievedAt);

        Assert.Equal("BRA", sportEvent.Participants[0].Country);
        Assert.Null(sportEvent.Participants[1].Country);
    }

    private static IReadOnlyList<SportEvent> FootballEvents(string file, WikipediaPageOptions page)
    {
        var boxes = ParsoidReader.ReadTemplates(Fixtures.Read("Wikipedia", file), FootballBoxMapper.IsFootballBox);

        return FootballBoxMapper.ToEvents(boxes, page, 2026, TestEvents.RetrievedAt);
    }
}
