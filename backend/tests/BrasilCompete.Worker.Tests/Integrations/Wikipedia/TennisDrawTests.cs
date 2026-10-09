using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia;
using BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

namespace BrasilCompete.Worker.Tests.Integrations.Wikipedia;

/// <summary>
/// Chaves do US Open 2026 (recorte das seções "Finals" e "Section 1") e do WTT Champions Macao 2026 (tênis de mesa,
/// só os títulos e as chaves).
/// </summary>
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

    private static readonly WikipediaPageOptions MacaoTableTennis = new()
    {
        Title = "WTT_Champions_Macao_2026",
        Sport = Sport.TableTennis,
        Competition = "WTT Champions Macau 2026",
        PeriodStart = new DateOnly(2026, 9, 8),
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
        var match = new TennisDrawMatch("Draw", "Section 1", 2, 4, 1, new TennisPlayer("João Fonseca", "João Fonseca (tennis)", "BRA"), null);

        var sportEvent = TennisDrawMapper.ToEvent(match, UsOpenMen, RetrievedAt);

        Assert.NotNull(sportEvent);
        Assert.Equal(EventFormat.Participation, sportEvent.Format);
        Assert.Equal("Section 1 · Rodada 2", sportEvent.Stage);
        Assert.Equal("US Open 2026 — simples masculino", sportEvent.Competition);
        Assert.Equal("BRA", Assert.Single(sportEvent.Participants).Country);
    }

    [Fact]
    public void ToEvent_Doubles_IsSkipped()
    {
        var first = new TennisPlayer("Hugo Calderano", "Hugo Calderano", "BRA");
        var match = new TennisDrawMatch("Mixed doubles", "Finals", 2, 2, 1, first, null);

        Assert.Null(TennisDrawMapper.ToEvent(match, MacaoTableTennis, RetrievedAt));
    }

    [Fact]
    public void TableTennis_FourPlayerFinals_AreSemifinalAndFinal()
    {
        var finals = MacaoMatches().Where(match => match.Event == "Men's singles" && match.Section == "Finals").ToList();

        var final = TennisDrawMapper.ToEvent(finals.Single(match => match.Round == 2), MacaoTableTennis, RetrievedAt);
        var semifinal = TennisDrawMapper.ToEvent(finals.First(match => match.Round == 1), MacaoTableTennis, RetrievedAt);

        Assert.NotNull(final);
        Assert.NotNull(semifinal);
        Assert.Equal(Sport.TableTennis, final.Sport);
        Assert.Equal("WTT Champions Macau 2026 — simples masculino", final.Competition);
        Assert.Equal("Final", final.Stage);
        Assert.Equal(["Hugo Calderano", "Truls Möregårdh"], final.Participants.Select(participant => participant.Name));
        Assert.Equal("Semifinal", semifinal.Stage);
        Assert.Equal(["Hugo Calderano", "Anton Källberg"], semifinal.Participants.Select(participant => participant.Name));
    }

    [Fact]
    public void TableTennis_ModuleBracket_IsReadFromPositionalParameters()
    {
        var section = MacaoMatches().Where(match => match.Event == "Men's singles" && match.Section == "Section 1").ToList();

        Assert.Equal(4, section.Count(match => match.Round == 1));
        var quarterfinal = Assert.Single(section, match => match.Round == 3);
        Assert.Equal("Sora Matsushima", quarterfinal.First?.Name);
        Assert.Equal("Hugo Calderano", quarterfinal.Second?.Name);
        Assert.Equal("BRA", quarterfinal.Second?.Country);
    }

    [Fact]
    public void RemoveRepeatedMatches_KeepsTheFinalsVersionOfAQuarterfinal()
    {
        var calderano = new TennisPlayer("Hugo Calderano", "Hugo Calderano", "BRA");
        var lebrun = new TennisPlayer("Félix Lebrun", "Félix Lebrun", "FRA");
        var other = new TennisPlayer("Dimitrij Ovtcharov", "Dimitrij Ovtcharov", "GER");
        var matches = new[]
        {
            new TennisDrawMatch("Men's singles", "Section 1", 1, 4, 1, other, calderano),
            new TennisDrawMatch("Men's singles", "Section 1", 4, 4, 1, lebrun, calderano),
            new TennisDrawMatch("Men's singles", "Finals", 1, 3, 1, calderano, lebrun),
        };

        var kept = TennisDrawReader.RemoveRepeatedMatches(matches);

        Assert.Equal(2, kept.Count);
        Assert.DoesNotContain(kept, match => match.Section == "Section 1" && match.Round == 4);
    }

    [Fact]
    public void TableTennis_WomensBrackets_KeepTheirEvent() =>
        Assert.Equal(5, MacaoMatches().Where(match => match.Event == "Women's singles").Select(match => match.Section).Distinct().Count());

    private static IReadOnlyList<TennisDrawMatch> Matches() =>
        TennisDrawReader.Read(Fixtures.Read("Wikipedia", "2026_US_Open_Mens_singles.html"));

    private static IReadOnlyList<TennisDrawMatch> MacaoMatches() =>
        TennisDrawReader.Read(Fixtures.Read("Wikipedia", "WTT_Champions_Macao_2026.html"));
}
