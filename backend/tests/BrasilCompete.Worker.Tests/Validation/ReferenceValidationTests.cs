using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Validation;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Validation;

public sealed class ReferenceValidationTests
{
    private const string Header = "modalidade,competicao,fase,participantes,visao,data,hora_brasilia,precisao_esperada,url_fonte_oficial,observacao";

    [Fact]
    public void Read_HandlesQuotedFieldsAndEmptyDates()
    {
        var csv = $""""
            {Header}
            football,Amistoso internacional,,Austrália x Brasil,Main,2026-09-25,07:00,Horário marcado,https://example.org,"empate, 1 a 1"
            tennis,,,João Fonseca,Main,,,,https://example.org,"sem jogos na janela; desistiu do ""US Open"""
            """";

        var rows = ReferenceCsvReader.Read(csv);

        Assert.Equal(2, rows.Count);
        Assert.Equal(["Austrália", "Brasil"], rows[0].Participants);
        Assert.Equal(new DateOnly(2026, 9, 25), rows[0].Date);
        Assert.Equal(new TimeOnly(7, 0), rows[0].Time);
        Assert.Equal("empate, 1 a 1", rows[0].Note);
        Assert.Null(rows[1].Date);
        Assert.Equal("sem jogos na janela; desistiu do \"US Open\"", rows[1].Note);
    }

    [Theory]
    [InlineData("Gabriel Bortoleto (Audi)", new[] { "Gabriel Bortoleto" })]
    [InlineData("Vinícius Júnior (Real Madrid) — Real Madrid x Espanyol", new[] { "Vinícius Júnior", "Real Madrid", "Espanyol" })]
    public void SplitParticipants_RemovesTeamsInParenthesesAndSplitsIndividuals(string value, string[] expected) =>
        Assert.Equal(expected, ReferenceCsvReader.SplitParticipants(value));

    [Fact]
    public void Match_UsesEnglishCountryNamesAndBrasiliaTime()
    {
        var sportEvent = Matchup(Sport.Football, At(2026, 9, 25, 10), "wikipedia", Athlete("Australia"), Athlete("Brazil"));

        var match = ReferenceMatcher.Match(Row("football", "Austrália x Brasil", new DateOnly(2026, 9, 25), new TimeOnly(7, 0)), [sportEvent]);

        Assert.True(match.Found);
        Assert.True(match.DateCorrect);
        Assert.True(match.TimeCorrect);
    }

    [Fact]
    public void Match_WrongTime_IsFoundButNotCorrect()
    {
        var sportEvent = Matchup(Sport.Football, At(2026, 9, 25, 12), "wikipedia", Athlete("Austrália"), Athlete("Brasil"));

        var match = ReferenceMatcher.Match(Row("football", "Austrália x Brasil", new DateOnly(2026, 9, 25), new TimeOnly(7, 0)), [sportEvent]);

        Assert.True(match.Found);
        Assert.False(match.TimeCorrect);
    }

    [Fact]
    public void Match_PeriodEvent_NeedsTheOpponentToTellTheMatchesApart()
    {
        var period = Schedule.InPeriod(new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 13));
        var firstRound = Matchup(Sport.TableTennis, period, "wikipedia", Athlete("Nicholas Lum"), Athlete("Hugo Calderano"));
        var final = Matchup(Sport.TableTennis, period, "wikipedia", Athlete("Hugo Calderano"), Athlete("Truls Möregårdh"));

        var match = ReferenceMatcher.Match(Row("table-tennis", "Hugo Calderano x Truls Moregardh", new DateOnly(2026, 9, 13), null), [firstRound, final]);

        Assert.Same(final, match.Event);
        Assert.Null(match.DateCorrect);
    }

    [Fact]
    public void Match_PrefersTheSameDayAmongSessions()
    {
        var qualifying = Participation(Sport.Formula1, At(2026, 9, 12, 14), "jolpica", "Classificação", Athlete("Gabriel Bortoleto"));
        var race = Participation(Sport.Formula1, At(2026, 9, 13, 13), "jolpica", "Corrida", Athlete("Gabriel Bortoleto"));

        var match = ReferenceMatcher.Match(Row("formula-1", "Gabriel Bortoleto (Audi)", new DateOnly(2026, 9, 13), new TimeOnly(10, 0)), [qualifying, race]);

        Assert.Same(race, match.Event);
        Assert.True(match.TimeCorrect);
    }

    [Fact]
    public void Match_OpponentWithAnotherName_IsFoundAndFlaggedWhenUnique()
    {
        var sportEvent = Matchup(Sport.Mma, Schedule.OnDate(new DateOnly(2026, 9, 19)), "wikipedia", Athlete("Patrício Pitbull"), Athlete("Choi Doo-ho"));

        var match = ReferenceMatcher.Match(Row("mma", "Patricio Pitbull x Dooho Choi", new DateOnly(2026, 9, 19), null), [sportEvent]);

        Assert.Same(sportEvent, match.Event);
        Assert.True(match.NameMismatch);
    }

    [Fact]
    public void Match_OnlyTheBrazilianInSeveralPeriodEvents_IsNotFound()
    {
        var period = Schedule.InPeriod(new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 13));
        var firstRound = Matchup(Sport.TableTennis, period, "wikipedia", Athlete("Nicholas Lum"), Athlete("Hugo Calderano"));
        var final = Matchup(Sport.TableTennis, period, "wikipedia", Athlete("Hugo Calderano"), Athlete("Truls Möregårdh"));

        Assert.False(ReferenceMatcher.Match(Row("table-tennis", "Hugo Calderano x Anton Källberg", new DateOnly(2026, 9, 12), null), [firstRound, final]).Found);
    }

    private static ReferenceRow Row(string sport, string participants, DateOnly date, TimeOnly? time) =>
        new(2, sport, "Competição", string.Empty, ReferenceCsvReader.SplitParticipants(participants), "Main", date, time, string.Empty, string.Empty, string.Empty);
}
