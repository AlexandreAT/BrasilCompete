using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Options;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Pipeline;

public sealed class EventDeduplicatorTests
{
    private readonly EventDeduplicator deduplicator = new(new SourcePriority(Options.Create(new WorkerOptions
    {
        SourcePriority = ["manual", "wikipedia", "thesportsdb"],
    })));

    [Fact]
    public void Merge_SameMatchFromTwoSources_KeepsBothSourcesAndUsesTheMorePreciseSchedule()
    {
        var fromWikipedia = Matchup(Sport.Football, Schedule.OnDate(new DateOnly(2026, 9, 16)), "wikipedia", Athlete("Santos"), Athlete("Peñarol"));
        var fromTheSportsDb = Matchup(Sport.Football, At(2026, 9, 17, 0), "thesportsdb", Athlete("Penarol"), Athlete("Santos"));

        var result = deduplicator.Merge([WithId(fromTheSportsDb), WithId(fromWikipedia)]);

        var merged = Assert.Single(result.Events);
        Assert.Equal(1, result.MergedCount);
        Assert.Equal(["wikipedia", "thesportsdb"], merged.Sources.Select(source => source.Source));
        Assert.Equal(SchedulePrecision.DateAndTime, merged.Schedule.Precision);
    }

    [Fact]
    public void Merge_ManualCurationWinsTheSchedule()
    {
        var manual = Matchup(Sport.Football, Schedule.OnDate(new DateOnly(2026, 9, 16)), "manual", Athlete("Santos"), Athlete("Barcelona"));
        var other = Matchup(Sport.Football, At(2026, 9, 16, 23), "thesportsdb", Athlete("Santos"), Athlete("Barcelona"));

        var merged = Assert.Single(deduplicator.Merge([WithId(other), WithId(manual)]).Events);

        Assert.Equal(SchedulePrecision.DateOnly, merged.Schedule.Precision);
    }

    [Fact]
    public void Merge_RecordsConflictingTimes()
    {
        var first = Matchup(Sport.Football, At(2026, 9, 16, 22), "wikipedia", Athlete("Santos"), Athlete("Barcelona"));
        var second = Matchup(Sport.Football, At(2026, 9, 16, 23), "thesportsdb", Athlete("Santos"), Athlete("Barcelona"));

        var result = deduplicator.Merge([WithId(first), WithId(second)]);

        var conflict = Assert.Single(result.Conflicts);
        Assert.Equal("startUtc", conflict.Field);
    }

    [Fact]
    public void Merge_DifferentSessionsOfTheSameDriver_StaySeparate()
    {
        var sprint = Participation(Sport.Formula1, At(2026, 11, 7, 14), "jolpica", "Sprint", Athlete("Piloto"));
        var qualifying = Participation(Sport.Formula1, At(2026, 11, 7, 18), "jolpica", "Classificação", Athlete("Piloto"));

        Assert.Equal(2, deduplicator.Merge([WithId(sprint), WithId(qualifying)]).Events.Count);
    }

    [Fact]
    public void Merge_TwoDistinctEventsFromTheSameSource_StaySeparate()
    {
        var first = Matchup(Sport.Chess, At(2026, 9, 20, 12), "lichess", Athlete("A"), Athlete("B")) with
        {
            Sources = [Source("lichess", "jogo-1")],
        };
        var second = first with { Sources = [Source("lichess", "jogo-2")] };

        Assert.Equal(2, deduplicator.Merge([WithId(first), WithId(second)]).Events.Count);
    }

    [Fact]
    public void Merge_MatchesMoreThanOneDayApart_StaySeparate()
    {
        var firstLeg = Matchup(Sport.Football, At(2026, 9, 16, 23), "wikipedia", Athlete("Santos"), Athlete("Barcelona"));
        var secondLeg = Matchup(Sport.Football, At(2026, 9, 23, 23), "wikipedia", Athlete("Barcelona"), Athlete("Santos"));

        Assert.Equal(2, deduplicator.Merge([WithId(firstLeg), WithId(secondLeg)]).Events.Count);
    }

    private static SportEvent WithId(SportEvent sportEvent) => sportEvent with { Id = EventIdFactory.Create(sportEvent) };
}
