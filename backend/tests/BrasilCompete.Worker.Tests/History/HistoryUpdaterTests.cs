using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.History;
using BrasilCompete.Worker.Pipeline;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.History;

public sealed class HistoryUpdaterTests
{
    private static readonly DateWindow Window = new(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));
    private static readonly DateTimeOffset FirstRun = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SecondRun = FirstRun.AddDays(1);

    [Fact]
    public void Apply_RecordsFirstAppearanceAndDate()
    {
        var history = new EventHistoryFile();
        var result = HistoryUpdater.Apply(history, [Santos(Schedule.OnDate(new DateOnly(2026, 9, 16)))], Window, FirstRun);

        var entry = Assert.Single(history.Events).Value;
        Assert.Equal(1, result.New);
        Assert.Equal(FirstRun, entry.FirstSeenUtc);
        Assert.Equal(FirstRun, entry.FirstSeenWithDateUtc);
    }

    [Fact]
    public void Apply_EventGainsADate_IsTheSameEventWithChanges()
    {
        var history = new EventHistoryFile();
        HistoryUpdater.Apply(history, [Santos(Schedule.ToBeConfirmed())], Window, FirstRun);

        var dated = Santos(At(2026, 9, 16, 23));
        var result = HistoryUpdater.Apply(history, [dated], Window, SecondRun);

        var entry = Assert.Single(history.Events);
        Assert.Equal(dated.Id, entry.Key);
        Assert.Equal(0, result.New);
        Assert.Equal(1, result.Changed);
        Assert.Equal(FirstRun, entry.Value.FirstSeenUtc);
        Assert.Equal(SecondRun, entry.Value.FirstSeenWithDateUtc);
        Assert.Contains(entry.Value.Changes, change => change.Field == "id");
        Assert.Contains(entry.Value.Changes, change => change.Field == "precision" && change.To == "DateAndTime");
    }

    [Fact]
    public void Apply_EventThatDisappears_IsMarkedAsMissingButKept()
    {
        var history = new EventHistoryFile();
        HistoryUpdater.Apply(history, [Santos(Schedule.OnDate(new DateOnly(2026, 9, 16)))], Window, FirstRun);

        var result = HistoryUpdater.Apply(history, [], Window, SecondRun);

        var entry = Assert.Single(history.Events).Value;
        Assert.Equal(1, result.Missing);
        Assert.Equal(SecondRun, entry.MissingSinceUtc);
    }

    [Fact]
    public void Apply_UnchangedEvent_IsCountedAsUnchanged()
    {
        var history = new EventHistoryFile();
        var sportEvent = Santos(At(2026, 9, 16, 23));
        HistoryUpdater.Apply(history, [sportEvent], Window, FirstRun);

        var result = HistoryUpdater.Apply(history, [sportEvent], Window, SecondRun);

        Assert.Equal(1, result.Unchanged);
        Assert.Empty(Assert.Single(history.Events).Value.Changes);
    }

    private static SportEvent Santos(Schedule schedule)
    {
        var sportEvent = Matchup(Sport.Football, schedule, "fonte", Athlete("Santos", "BRA"), Athlete("Barcelona", "ESP"));

        return sportEvent with { Id = EventIdFactory.Create(sportEvent) };
    }
}
