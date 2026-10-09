namespace BrasilCompete.Worker.Normalization;

public static class LocalTimeConverter
{
    /// <summary>
    /// Converte data e hora locais de um fuso IANA em um instante com offset.
    /// Horários ambíguos (fim do horário de verão) usam o horário padrão.
    /// </summary>
    public static DateTimeOffset ToInstant(DateOnly date, TimeOnly time, string ianaTimeZone)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);

        if (zone.IsInvalidTime(local))
        {
            throw new ArgumentException(
                $"O horário {local:yyyy-MM-dd HH:mm} não existe no fuso {ianaTimeZone}.",
                nameof(time));
        }

        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
