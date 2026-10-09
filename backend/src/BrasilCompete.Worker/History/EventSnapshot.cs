using System.Globalization;

using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.History;

/// <summary>Os campos de um evento que o histórico acompanha entre execuções.</summary>
public sealed record EventSnapshot
{
    public required SchedulePrecision Precision { get; init; }

    public DateTimeOffset? StartUtc { get; init; }

    public DateOnly? Date { get; init; }

    public DateOnly? PeriodStart { get; init; }

    public DateOnly? PeriodEnd { get; init; }

    public string? Stage { get; init; }

    public string? Venue { get; init; }

    public required string Participants { get; init; }

    public required string Sources { get; init; }

    /// <summary>Dia de referência, para escolher o evento mais próximo quando o identificador muda.</summary>
    public DateOnly? ReferenceDate => Date ?? PeriodStart;

    public static EventSnapshot From(SportEvent sportEvent) => new()
    {
        Precision = sportEvent.Schedule.Precision,
        StartUtc = sportEvent.Schedule.StartUtc,
        Date = sportEvent.Schedule.Date,
        PeriodStart = sportEvent.Schedule.PeriodStart,
        PeriodEnd = sportEvent.Schedule.PeriodEnd,
        Stage = sportEvent.Stage,
        Venue = sportEvent.Venue,
        Participants = string.Join(" x ", sportEvent.Participants.Select(participant => participant.Name)),
        Sources = string.Join(", ", sportEvent.Sources.Select(source => source.Source).Distinct().Order(StringComparer.Ordinal)),
    };

    public IEnumerable<(string Field, string? From, string? To)> Diff(EventSnapshot current)
    {
        var pairs = new (string Field, string? From, string? To)[]
        {
            ("precision", Precision.ToString(), current.Precision.ToString()),
            ("startUtc", Format(StartUtc), Format(current.StartUtc)),
            ("date", Format(Date), Format(current.Date)),
            ("periodStart", Format(PeriodStart), Format(current.PeriodStart)),
            ("periodEnd", Format(PeriodEnd), Format(current.PeriodEnd)),
            ("stage", Stage, current.Stage),
            ("venue", Venue, current.Venue),
            ("participants", Participants, current.Participants),
            ("sources", Sources, current.Sources),
        };

        return pairs.Where(pair => !string.Equals(pair.From, pair.To, StringComparison.Ordinal));
    }

    private static string? Format(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private static string? Format(DateOnly? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
