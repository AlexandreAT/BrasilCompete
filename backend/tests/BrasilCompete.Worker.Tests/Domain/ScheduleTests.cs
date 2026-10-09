using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Tests.Domain;

public sealed class ScheduleTests
{
    [Fact]
    public void AtTime_UsesTheBrasiliaDate()
    {
        var schedule = Schedule.AtTime(DateTimeOffset.Parse("2026-10-15T01:00:00Z"), "Etc/UTC");

        Assert.Equal(SchedulePrecision.DateAndTime, schedule.Precision);
        Assert.Equal(new DateOnly(2026, 10, 14), schedule.Date);
        Assert.Equal(TimeSpan.Zero, schedule.StartUtc!.Value.Offset);
    }

    [Fact]
    public void InPeriod_NeverEndsBeforeItStarts()
    {
        var schedule = Schedule.InPeriod(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 18));

        Assert.Equal(schedule.PeriodStart, schedule.PeriodEnd);
    }

    [Fact]
    public void GetDateSpan_ReturnsNullWhenNothingIsDefined() =>
        Assert.Null(Schedule.ToBeConfirmed().GetDateSpan());

    [Theory]
    [InlineData("2026-09-30", "2026-10-01", true)]
    [InlineData("2026-10-31", "2026-11-02", true)]
    [InlineData("2026-09-20", "2026-09-30", false)]
    [InlineData("2026-11-01", "2026-11-03", false)]
    public void DateWindow_OverlapsPeriods(string start, string end, bool expected)
    {
        var window = new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
        var schedule = Schedule.InPeriod(DateOnly.Parse(start), DateOnly.Parse(end));

        Assert.Equal(expected, window.Overlaps(schedule));
    }

    [Fact]
    public void DateWindow_AlwaysContainsEventsToBeConfirmed()
    {
        var window = new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.True(window.Overlaps(Schedule.ToBeConfirmed()));
    }
}
