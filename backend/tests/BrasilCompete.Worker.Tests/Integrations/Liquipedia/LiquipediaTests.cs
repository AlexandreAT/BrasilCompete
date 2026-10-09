using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Liquipedia;
using BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

namespace BrasilCompete.Worker.Tests.Integrations.Liquipedia;

/// <summary>
/// Respostas reais da API da Liquipedia salvas em 09/10/2026: o VALORANT Champions 2026 (partidas em páginas
/// <c>Match:</c>), a StarLadder StarSeries Fall 2026 de CS2 (partidas dentro da chave) e a tabela de fusos.
/// </summary>
public sealed class LiquipediaTests
{
    private static readonly LiquipediaTournamentOptions Champions = new()
    {
        Wiki = "valorant",
        Page = "VCT/2026/Champions",
        Sport = Sport.EsportsValorant,
        Competition = "VALORANT Champions 2026",
    };

    [Fact]
    public void Timezones_ComeFromTheLiquipediaModule()
    {
        var timezones = Timezones();

        Assert.Equal(TimeSpan.FromHours(-3), timezones["BRT"]);
        Assert.Equal(TimeSpan.FromHours(2), timezones["CEST"]);
        Assert.Equal(TimeSpan.FromHours(9) + TimeSpan.FromMinutes(30), timezones["ACST"]);
        Assert.True(timezones.ContainsKey("CST"));
    }

    [Fact]
    public void ReadStages_MapsBracketIdsToTheirTitles()
    {
        var stages = LiquipediaWikitextReader.ReadStages(Content("valorant-VCT_2026_Champions.json").Single());

        Assert.Equal("Group A Matches", stages["CHAMP26GrA"]);
        Assert.Equal("Playoffs", stages["CHAMP26PLF"]);
    }

    [Fact]
    public void ReadMatch_LoudVersusEdwardGaming_HasDateTimezoneAndTeams()
    {
        var page = Pages("valorant-champions-2026-matches.json").Single(item => item.Title == "Match:ID CHAMP26GrB 0002");

        var match = LiquipediaWikitextReader.ReadMatch(page.Title, page.Content!)!;

        Assert.Equal("CHAMP26GrB", match.BracketId);
        Assert.Equal(new DateOnly(2026, 9, 26), match.Date);
        Assert.Equal(new TimeOnly(20, 30), match.LocalTime);
        Assert.Equal("CST", match.TimezoneAbbreviation);
        Assert.Equal(("LOUD", "EDward Gaming"), (match.FirstTeam, match.SecondTeam));
    }

    [Fact]
    public void ToEvent_UsesTheLiquipediaOffsetAndTheTeamCountry()
    {
        var page = Pages("valorant-champions-2026-matches.json").Single(item => item.Title == "Match:ID CHAMP26GrB 0002");
        var match = LiquipediaWikitextReader.ReadMatch(page.Title, page.Content!)!;
        var teams = new Dictionary<string, LiquipediaTeam>
        {
            ["LOUD"] = new("LOUD", "Brazil"),
            ["EDward Gaming"] = new("EDward Gaming", "China"),
        };

        var sportEvent = LiquipediaMapper.ToEvent(match, Champions, new Dictionary<string, string> { ["CHAMP26GrB"] = "Group B Matches" }, teams, Timezones(), TestEvents.RetrievedAt)!;

        var expectedStart = new DateTimeOffset(2026, 9, 26, 20, 30, 0, Timezones()["CST"]);
        Assert.Equal(expectedStart, sportEvent.Schedule.StartUtc);
        Assert.Equal("BRA", sportEvent.Participants[0].Country);
        Assert.Equal(ParticipantKind.Organization, sportEvent.Participants[0].Kind);
        Assert.Equal("Group B Matches", sportEvent.Stage);
    }

    [Fact]
    public void ReadInlineMatches_StarSeries_FindsFuriaVersusMibr()
    {
        var content = Content("counterstrike-StarSeries_2026_Fall.json").Single();

        var matches = LiquipediaWikitextReader.ReadInlineMatches("StarLadder/StarSeries/2026/Fall", content);

        // FURIA x MIBR se enfrentaram duas vezes: na chave superior (17/09) e na inferior (19/09).
        var meetings = matches.Where(item => item.FirstTeam == "furia" && item.SecondTeam == "mibr").ToList();
        Assert.Equal([new DateOnly(2026, 9, 17), new DateOnly(2026, 9, 19)], meetings.Select(item => item.Date!.Value).Order());
        var first = meetings.Single(item => item.Date == new DateOnly(2026, 9, 17));
        Assert.Equal(new TimeOnly(19, 10), first.LocalTime);
        Assert.Equal("CEST", first.TimezoneAbbreviation);
        Assert.Equal(5, matches.Count(item => item.FirstTeam == "furia" || item.SecondTeam == "furia"));
    }

    [Theory]
    [InlineData(new[] { "CHAMP26GrA", "CHAMP26GrB", "CHAMP26PLF" }, new[] { "ID_CHAMP26" })]
    [InlineData(new[] { "yXXrhvYm7l", "abcDEF1234" }, new[] { "ID_yXXrhvYm7l", "ID_abcDEF1234" })]
    public void MatchPagePrefixes_UseTheCommonPrefixWhenPossible(string[] ids, string[] expected) =>
        Assert.Equal(expected, LiquipediaEventSource.MatchPagePrefixes(ids));

    [Fact]
    public void BuildTeamPageText_WritesOneTemplatePerLine() =>
        Assert.Equal("{{TeamPage|furia}}\n{{TeamPage|vit}}", LiquipediaWikitextReader.BuildTeamPageText(["furia", "vit"]));

    [Fact]
    public void ParseTeamPages_MapsEachNameToItsPageAndSkipsUnknownNames()
    {
        // Resposta real do expandtemplates (09/10/2026), com um nome sem predefinição.
        var expanded = "FURIA\nTeam Vitality\n<div class=\"error\">No team template exists for name \"zzz\".</div>";

        var titles = LiquipediaWikitextReader.ParseTeamPages(["furia", "vit", "zzz"], expanded);

        Assert.Equal("FURIA", titles["furia"]);
        Assert.Equal("Team Vitality", titles["vit"]);
        Assert.False(titles.ContainsKey("zzz"));
    }

    [Fact]
    public void ParseTeamPages_LineCountMismatch_UsesNothing() =>
        Assert.Empty(LiquipediaWikitextReader.ParseTeamPages(["furia", "vit"], "FURIA"));

    private static IReadOnlyDictionary<string, TimeSpan> Timezones() =>
        LiquipediaTimezones.Parse(Content("commons-Module_Timezone_Data.json").Single());

    private static IEnumerable<string> Content(string file) => Pages(file).Select(page => page.Content!);

    private static IReadOnlyList<LiquipediaPageResponse> Pages(string file) =>
        Fixtures.ReadJson<LiquipediaQueryResponse>("Liquipedia", file).Query!.Pages;
}
