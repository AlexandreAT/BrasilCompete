namespace BrasilCompete.Worker.Domain;

/// <summary>
/// A agenda é exibida no horário de Brasília (o Brasil não tem horário de verão desde 2019).
/// </summary>
public static class BrasiliaTime
{
    public const string TimeZoneId = "America/Sao_Paulo";

    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public static DateTime ToLocal(DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, Zone).DateTime;

    public static DateOnly ToDate(DateTimeOffset value) => DateOnly.FromDateTime(ToLocal(value));
}
