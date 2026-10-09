namespace BrasilCompete.Worker.Domain;

/// <summary>
/// Quando o evento acontece, com exatamente um nível de precisão (plano, seção 4.4).
/// Em eventos com horário, <see cref="Date"/> é a data no horário de Brasília.
/// </summary>
public sealed record Schedule
{
    public required SchedulePrecision Precision { get; init; }

    public DateTimeOffset? StartUtc { get; init; }

    public DateOnly? Date { get; init; }

    public DateOnly? PeriodStart { get; init; }

    public DateOnly? PeriodEnd { get; init; }

    public string? OriginalTimeZone { get; init; }

    public static Schedule AtTime(DateTimeOffset start, string originalTimeZone) => new()
    {
        Precision = SchedulePrecision.DateAndTime,
        StartUtc = start.ToUniversalTime(),
        Date = BrasiliaTime.ToDate(start),
        OriginalTimeZone = originalTimeZone,
    };

    public static Schedule OnDate(DateOnly date) => new()
    {
        Precision = SchedulePrecision.DateOnly,
        Date = date,
    };

    public static Schedule InPeriod(DateOnly start, DateOnly end) => new()
    {
        Precision = SchedulePrecision.CompetitionPeriod,
        PeriodStart = start,
        PeriodEnd = end < start ? start : end,
    };

    public static Schedule ToBeConfirmed() => new()
    {
        Precision = SchedulePrecision.ToBeConfirmed,
    };

    /// <summary>
    /// Primeiro e último dia em que o evento pode acontecer, ou <c>null</c> quando nada está definido.
    /// </summary>
    public (DateOnly First, DateOnly Last)? GetDateSpan() => Precision switch
    {
        SchedulePrecision.DateAndTime or SchedulePrecision.DateOnly when Date is { } date => (date, date),
        SchedulePrecision.CompetitionPeriod when PeriodStart is { } start && PeriodEnd is { } end => (start, end),
        _ => null,
    };
}
