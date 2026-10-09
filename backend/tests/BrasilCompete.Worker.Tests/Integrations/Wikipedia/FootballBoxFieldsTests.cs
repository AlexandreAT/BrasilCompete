using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia.Football;

namespace BrasilCompete.Worker.Tests.Integrations.Wikipedia;

/// <summary>Formatos reais encontrados nas páginas da Wikipedia em 09/10/2026.</summary>
public sealed class FootballBoxFieldsTests
{
    [Theory]
    [InlineData("{{Start date|2026|8|11|df=y}}", null, "2026-08-11")]
    [InlineData("18 November 2025", null, "2025-11-18")]
    [InlineData("10 October", 2026, "2026-10-10")]
    [InlineData("August 11, 2026", null, "2026-08-11")]
    [InlineData("{{dts|2026|9|16}}", null, "2026-09-16")]
    public void ParseDate_ReadsTheCommonFormats(string wikitext, int? year, string expected) =>
        Assert.Equal(DateOnly.Parse(expected), FootballBoxFields.ParseDate(wikitext, year));

    [Theory]
    [InlineData("10 October", null)]
    [InlineData("TBD", 2026)]
    public void ParseDate_WithoutYearOrDate_IsNull(string wikitext, int? year) =>
        Assert.Null(FootballBoxFields.ParseDate(wikitext, year));

    [Theory]
    [InlineData("Football box", true)]
    [InlineData("Football box collapsible", true)]
    [InlineData("footballbox collapsible", true)]
    [InlineData("Footballbox_collapsible", true)]
    [InlineData("Football kit", false)]
    public void IsFootballBox_AcceptsTheRedirectsWithoutSpace(string templateName, bool expected) =>
        Assert.Equal(expected, FootballBoxMapper.IsFootballBox(templateName));

    [Theory]
    [InlineData("{{UTZ|21:30|-3}}", "21:30", -3)]
    [InlineData("{{UTZ|20:30|1}}", "20:30", 1)]
    [InlineData("{{UTZ|19:00|−4}}", "19:00", -4)]
    [InlineData("21:30 [[UTC−03:00|UTC−3]]", "21:30", -3)]
    public void ParseTime_ReadsTimeAndOffset(string wikitext, string time, int offsetHours)
    {
        var parsed = FootballBoxFields.ParseTime(wikitext);

        Assert.Equal(TimeOnly.Parse(time), parsed!.Value.Time);
        Assert.Equal(TimeSpan.FromHours(offsetHours), parsed.Value.Offset);
    }

    [Theory]
    [InlineData("{{UTZ|--:--|-3}}")]
    [InlineData("21:30 local time")]
    [InlineData("")]
    public void ParseTime_WithoutKnownTimeOrOffset_IsNull(string wikitext) =>
        Assert.Null(FootballBoxFields.ParseTime(wikitext));

    [Fact]
    public void HasTimeWithoutOffset_DetectsTimesThatCannotBeUsed()
    {
        Assert.True(FootballBoxFields.HasTimeWithoutOffset("21:30 local time"));
        Assert.False(FootballBoxFields.HasTimeWithoutOffset("{{UTZ|21:30|-3}}"));
    }

    [Fact]
    public void ParseTeam_NationalTeamTemplates()
    {
        var men = FootballBoxFields.ParseTeam("{{fb-rt|BRA}}")!;
        var women = FootballBoxFields.ParseTeam("{{fbw|ARG}}")!;
        var youth = FootballBoxFields.ParseTeam("{{fbu|BRA|20}}")!;

        Assert.Equal(("Brasil", ParticipantKind.NationalTeam, "BRA"), (men.Name, men.Kind, men.Country));
        Assert.Equal("Argentina (feminino)", women.Name);
        Assert.Equal("Brasil Sub-20", youth.Name);
    }

    [Fact]
    public void ParseTeam_ClubWithIcon_HasCountryAndWikipediaArticle()
    {
        var club = FootballBoxFields.ParseTeam("{{fbaicon|CHI}} [[Club Deportivo Universidad Católica|Universidad Católica]]")!;

        Assert.Equal("Universidad Católica", club.Name);
        Assert.Equal(ParticipantKind.Club, club.Kind);
        Assert.Equal("CHI", club.Country);
        Assert.Equal("Club Deportivo Universidad Católica", club.ExternalIds[ExternalIdKeys.EnglishWikipedia]);
    }

    [Theory]
    [InlineData("TBD")]
    [InlineData("Winner of Match QF1")]
    [InlineData("")]
    public void ParseTeam_UndefinedOpponent_IsNull(string wikitext) =>
        Assert.Null(FootballBoxFields.ParseTeam(wikitext));
}
