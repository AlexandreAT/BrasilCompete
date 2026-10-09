using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia;
using BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

namespace BrasilCompete.Worker.Tests.Integrations.Wikipedia;

/// <summary>Chave de simples do US Open 2026 (recorte das seções "Finals" e "Section 1").</summary>
public sealed class TennisDrawTests
{
    private static readonly WikipediaPageOptions UsOpenMen = new()
    {
        Title = "2026_US_Open_–_Men's_singles",
        Sport = Sport.Tennis,
        Competition = "US Open 2026 — simples masculino",
        PeriodStart = new DateOnly(2026, 8, 30),
        PeriodEnd = new DateOnly(2026, 9, 13),
    };

    private static readonly DateTimeOffset RetrievedAt = DateTimeOffset.Parse("2026-10-09T12:00:00Z");

    [Fact]
    public void Read_FinalsBracket_PairsQuarterfinalsSemifinalsAndFinal()
    {
        var finals = Matches().Where(match => match.Section == "Finals").ToList();

        Assert.Equal(4, finals.Count(match => match.Round == 1));
        Assert.Equal(2, finals.Count(match => match.Round == 2));

        var final = Assert.Single(finals, match => match.Round == 3);
        Assert.Equal("Alexander Zverev", final.First?.Name);
        Assert.Equal("GER", final.First?.Country);
        Assert.Equal("Ben Shelton", final.Second?.Name);
    }

    [Fact]
    public void Read_SectionBracket_KeepsTheSectionHeading()
    {
        var firstRound = Matches().Where(match => match.Section == "Section 1" && match.Round == 1).ToList();

        Assert.Equal(8, firstRound.Count);
        Assert.Equal("Lorenzo Sonego", firstRound[0].Second?.Name);
        Assert.Equal("ITA", firstRound[0].Second?.Country);
    }

    [Fact]
    public void Read_NeutralAthlete_HasNoCountry()
    {
        var quarterfinal = Matches().First(match => match.Section == "Finals" && match.Round == 1 && match.Slot == 3);

        Assert.Equal("Karen Khachanov", quarterfinal.First?.Name);
        Assert.Null(quarterfinal.First?.Country);
    }

    [Theory]
    [InlineData("{{flagicon|BRA}} [[João Fonseca (tennis)|J Fonseca]]", "João Fonseca", "João Fonseca (tennis)", "BRA")]
    [InlineData("'''{{flagIOCathlete|[[Beatriz Haddad Maia|B Haddad Maia]]|BRA}}'''", "Beatriz Haddad Maia", "Beatriz Haddad Maia", "BRA")]
    [InlineData("{{flagicon|}} [[Karen Khachanov]]", "Karen Khachanov", "Karen Khachanov", null)]
    public void ParsePlayer_ReadsNameTitleAndFlag(string wikitext, string name, string title, string? country)
    {
        var player = TennisDrawReader.ParsePlayer(wikitext);

        Assert.NotNull(player);
        Assert.Equal(name, player.Name);
        Assert.Equal(title, player.WikipediaTitle);
        Assert.Equal(country, player.Country);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bye")]
    [InlineData("{{flagicon|BRA}} Qualifier")]
    public void ParsePlayer_EmptyOrUndefined_ReturnsNull(string wikitext)
    {
        Assert.Null(TennisDrawReader.ParsePlayer(wikitext));
    }

    [Fact]
    public void ToEvent_UsesTheTournamentPeriodAndTheRoundInPortuguese()
    {
        var final = Matches().Single(match => match.Section == "Finals" && match.Round == 3);

        var sportEvent = TennisDrawMapper.ToEvent(final, UsOpenMen, RetrievedAt);

        Assert.NotNull(sportEvent);
        Assert.Equal(EventFormat.Matchup, sportEvent.Format);
        Assert.Equal("Final", sportEvent.Stage);
        Assert.Equal(SchedulePrecision.CompetitionPeriod, sportEvent.Schedule.Precision);
        Assert.Equal(new DateOnly(2026, 8, 30), sportEvent.Schedule.PeriodStart);
        Assert.Equal(new DateOnly(2026, 9, 13), sportEvent.Schedule.PeriodEnd);
        Assert.Equal("Alexander Zverev", sportEvent.Participants[0].ExternalIds[ExternalIdKeys.EnglishWikipedia]);
    }

    [Fact]
    public void ToEvent_OneSideUndefined_BecomesParticipation()
    {
        var match = new TennisDrawMatch("Section 1", 2, 1, new TennisPlayer("João Fonseca", "João Fonseca (tennis)", "BRA"), null);

        var sportEvent = TennisDrawMapper.ToEvent(match, UsOpenMen, RetrievedAt);

        Assert.NotNull(sportEvent);
        Assert.Equal(EventFormat.Participation, sportEvent.Format);
        Assert.Equal("Section 1 · Rodada 2", sportEvent.Stage);
        Assert.Equal("BRA", Assert.Single(sportEvent.Participants).Country);
    }

    private static IReadOnlyList<TennisDrawMatch> Matches() =>
        TennisDrawReader.Read(Fixtures.Read("Wikipedia", "2026_US_Open_Mens_singles.html"));
}
