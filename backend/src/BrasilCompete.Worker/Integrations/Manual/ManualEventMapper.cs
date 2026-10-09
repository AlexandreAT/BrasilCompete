using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Manual;

public static class ManualEventMapper
{
    public const string License = "Curadoria própria (somente fatos)";

    public static SportEvent ToSportEvent(ManualEventEntry entry, DateTimeOffset retrievedAtUtc) => new()
    {
        Sport = entry.Sport,
        Competition = entry.Competition,
        Stage = entry.Stage,
        Format = entry.Format,
        Scope = entry.Scope,
        Schedule = ToSchedule(entry.Schedule, entry.Key),
        Venue = entry.Venue,
        Participants = entry.Participants.Select(ToParticipant).ToList(),
        Sources =
        [
            new SourceReference
            {
                Source = ManualEventSource.SourceName,
                Url = entry.SourceUrl,
                License = License,
                RetrievedAtUtc = retrievedAtUtc,
                ExternalId = entry.Key,
            },
        ],
        IsTestData = entry.IsTestData,
    };

    private static Schedule ToSchedule(ManualScheduleEntry schedule, string key) => schedule.Precision switch
    {
        SchedulePrecision.DateAndTime => Schedule.AtTime(
            LocalTimeConverter.ToInstant(
                Require(schedule.Date, key, "date"),
                TimeOnly.ParseExact(Require(schedule.LocalTime, key, "localTime"), "HH:mm", CultureInfo.InvariantCulture),
                Require(schedule.TimeZone, key, "timeZone")),
            schedule.TimeZone!),
        SchedulePrecision.DateOnly => Schedule.OnDate(Require(schedule.Date, key, "date")),
        SchedulePrecision.CompetitionPeriod => Schedule.InPeriod(
            Require(schedule.PeriodStart, key, "periodStart"),
            Require(schedule.PeriodEnd, key, "periodEnd")),
        _ => Schedule.ToBeConfirmed(),
    };

    private static Participant ToParticipant(ManualParticipantEntry entry) => new()
    {
        Name = entry.Name,
        Kind = entry.Kind,
        Country = entry.Country,
        Team = entry.Team,
        IsBrazilian = entry.IsBrazilian ?? false,
        BrazilianReason = entry.IsBrazilian == true ? entry.BrazilianReason : null,
        DecidedBy = entry.IsBrazilian is null ? null : IdentityLayer.Manual,
        ExternalIds = entry.ExternalIds ?? new Dictionary<string, string>(),
        Members = entry.Members?.Select(ToParticipant).ToList() ?? [],
    };

    private static T Require<T>(T? value, string key, string field) where T : struct =>
        value ?? throw new InvalidDataException($"A entrada '{key}' da curadoria não tem o campo '{field}'.");

    private static string Require(string? value, string key, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidDataException($"A entrada '{key}' da curadoria não tem o campo '{field}'.")
            : value;
}
