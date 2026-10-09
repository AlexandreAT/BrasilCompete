using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Pipeline;

public sealed class EventIdFactoryTests
{
    [Fact]
    public void Create_IsDeterministicAndIgnoresParticipantOrder()
    {
        var first = Matchup(Sport.EsportsValorant, At(2026, 9, 20, 17), "fonte", Athlete("LOUD"), Athlete("Equipe Exemplo"));
        var second = Matchup(Sport.EsportsValorant, At(2026, 9, 20, 17), "outra", Athlete("Equipe Exemplo"), Athlete("LOUD"));

        Assert.Equal(EventIdFactory.Create(first), EventIdFactory.Create(second));
        Assert.Equal("esports-valorant:competicao-de-teste:equipe-exemplo-x-loud:2026-09-20", EventIdFactory.Create(first));
    }

    [Fact]
    public void Create_UsesThePeriodStartOrToBeConfirmed()
    {
        var period = Participation(Sport.TableTennis, Schedule.InPeriod(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 9)), "fonte", null, Athlete("Hugo Calderano"));
        var unknown = Matchup(Sport.Football, Schedule.ToBeConfirmed(), "fonte", Athlete("Santos"), Athlete("Barcelona"));

        Assert.EndsWith(":2026-11-04", EventIdFactory.Create(period));
        Assert.EndsWith(":a-confirmar", EventIdFactory.Create(unknown));
    }

    [Fact]
    public void CreateMatchKey_IncludesTheStageButNotTheDate()
    {
        var race = Participation(Sport.Formula1, At(2026, 11, 8, 17), "fonte", "Corrida", Athlete("Piloto"));

        Assert.Equal("formula-1:competicao-de-teste:piloto:corrida", EventIdFactory.CreateMatchKey(race));
    }
}
