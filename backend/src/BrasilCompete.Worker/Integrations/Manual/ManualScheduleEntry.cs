using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations.Manual;

/// <summary>
/// Horário como a pessoa encontrou na fonte: data e hora locais (<c>HH:mm</c>) e o fuso IANA do local.
/// </summary>
public sealed record ManualScheduleEntry
{
    public required SchedulePrecision Precision { get; init; }

    public DateOnly? Date { get; init; }

    public string? LocalTime { get; init; }

    public string? TimeZone { get; init; }

    public DateOnly? PeriodStart { get; init; }

    public DateOnly? PeriodEnd { get; init; }
}
