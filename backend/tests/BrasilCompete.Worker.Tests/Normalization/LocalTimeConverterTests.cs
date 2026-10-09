using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Tests.Normalization;

/// <summary>Viradas de horário de verão de 2026: Europa em 25/10 e Estados Unidos em 01/11 (plano, seção 9.8).</summary>
public sealed class LocalTimeConverterTests
{
    [Theory]
    [InlineData("Europe/London", "2026-10-24", "15:00", "2026-10-24T14:00:00Z")]
    [InlineData("Europe/London", "2026-10-25", "15:00", "2026-10-25T15:00:00Z")]
    [InlineData("Europe/Paris", "2026-10-24", "21:00", "2026-10-24T19:00:00Z")]
    [InlineData("Europe/Paris", "2026-10-26", "21:00", "2026-10-26T20:00:00Z")]
    [InlineData("America/New_York", "2026-10-31", "20:00", "2026-11-01T00:00:00Z")]
    [InlineData("America/New_York", "2026-11-01", "20:00", "2026-11-02T01:00:00Z")]
    [InlineData("America/Sao_Paulo", "2026-11-01", "20:00", "2026-11-01T23:00:00Z")]
    public void ToInstant_AppliesTheOffsetOfThatDate(string zone, string date, string time, string expectedUtc)
    {
        var instant = LocalTimeConverter.ToInstant(DateOnly.Parse(date), TimeOnly.Parse(time), zone);

        Assert.Equal(DateTimeOffset.Parse(expectedUtc), instant);
    }

    [Fact]
    public void ToInstant_SameLocalTimeInParis_ShiftsOneHourInBrasiliaAfterTheEuropeanChange()
    {
        var before = LocalTimeConverter.ToInstant(new DateOnly(2026, 10, 24), new TimeOnly(21, 0), "Europe/Paris");
        var after = LocalTimeConverter.ToInstant(new DateOnly(2026, 10, 26), new TimeOnly(21, 0), "Europe/Paris");

        Assert.Equal(new TimeOnly(16, 0), TimeOnly.FromDateTime(BrasiliaTime.ToLocal(before)));
        Assert.Equal(new TimeOnly(17, 0), TimeOnly.FromDateTime(BrasiliaTime.ToLocal(after)));
    }

    [Fact]
    public void ToInstant_AmbiguousTime_UsesStandardTime()
    {
        var instant = LocalTimeConverter.ToInstant(new DateOnly(2026, 10, 25), new TimeOnly(2, 30), "Europe/Paris");

        Assert.Equal(DateTimeOffset.Parse("2026-10-25T01:30:00Z"), instant);
    }

    [Fact]
    public void ToInstant_TimeSkippedBySpringForward_Throws() =>
        Assert.Throws<ArgumentException>(() =>
            LocalTimeConverter.ToInstant(new DateOnly(2026, 3, 29), new TimeOnly(2, 30), "Europe/Paris"));
}
