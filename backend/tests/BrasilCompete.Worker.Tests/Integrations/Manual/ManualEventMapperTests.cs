using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Manual;

namespace BrasilCompete.Worker.Tests.Integrations.Manual;

public sealed class ManualEventMapperTests
{
    [Fact]
    public void ToSportEvent_ConvertsTheLocalTimeAndKeepsTheManualDecision()
    {
        var entry = new ManualEventEntry
        {
            Key = "exemplo",
            Sport = Sport.Football,
            Competition = "Amistoso",
            Format = EventFormat.Matchup,
            Scope = CompetitionScope.International,
            Schedule = new ManualScheduleEntry
            {
                Precision = SchedulePrecision.DateAndTime,
                Date = new DateOnly(2026, 10, 14),
                LocalTime = "21:00",
                TimeZone = "Europe/Paris",
            },
            Participants =
            [
                new ManualParticipantEntry { Name = "Brasil", Kind = ParticipantKind.NationalTeam, IsBrazilian = true, BrazilianReason = BrazilianReason.BrazilianNationalTeam },
                new ManualParticipantEntry { Name = "França", Kind = ParticipantKind.NationalTeam, Country = "FRA" },
            ],
            SourceUrl = "https://example.org",
        };

        var sportEvent = ManualEventMapper.ToSportEvent(entry, TestEvents.RetrievedAt);

        Assert.Equal(DateTimeOffset.Parse("2026-10-14T19:00:00Z"), sportEvent.Schedule.StartUtc);
        Assert.Equal("Europe/Paris", sportEvent.Schedule.OriginalTimeZone);
        Assert.Equal(IdentityLayer.Manual, sportEvent.Participants[0].DecidedBy);
        Assert.Null(sportEvent.Participants[1].DecidedBy);
        Assert.Equal("manual", Assert.Single(sportEvent.Sources).Source);
    }

    [Fact]
    public void ToSportEvent_MissingField_ExplainsWhichEntry()
    {
        var entry = new ManualEventEntry
        {
            Key = "sem-data",
            Sport = Sport.Tennis,
            Competition = "Torneio",
            Format = EventFormat.Participation,
            Scope = CompetitionScope.International,
            Schedule = new ManualScheduleEntry { Precision = SchedulePrecision.DateOnly },
            Participants = [],
            SourceUrl = "https://example.org",
        };

        var exception = Assert.Throws<InvalidDataException>(() => ManualEventMapper.ToSportEvent(entry, TestEvents.RetrievedAt));

        Assert.Contains("sem-data", exception.Message);
    }
}
