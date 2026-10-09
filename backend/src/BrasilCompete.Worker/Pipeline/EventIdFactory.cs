using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Identificador determinístico, para o histórico reconhecer o mesmo evento entre execuções.
/// Formato: <c>modalidade:competição:participantes[:fase]:data</c>.
/// </summary>
public static class EventIdFactory
{
    public static string Create(SportEvent sportEvent) =>
        $"{CreateMatchKey(sportEvent)}:{DatePart(sportEvent.Schedule)}";

    /// <summary>O identificador sem a data, usado quando o evento muda de data ou de precisão.</summary>
    public static string CreateMatchKey(SportEvent sportEvent)
    {
        var names = sportEvent.Participants
            .Select(participant => TextNormalizer.Slugify(participant.Name))
            .Order(StringComparer.Ordinal);
        var separator = sportEvent.Format == EventFormat.Matchup ? "-x-" : "-";
        var key = $"{sportEvent.Sport.ToSlug()}:{TextNormalizer.Slugify(sportEvent.Competition)}:{string.Join(separator, names)}";

        return sportEvent.Stage is null ? key : $"{key}:{TextNormalizer.Slugify(sportEvent.Stage)}";
    }

    private static string DatePart(Schedule schedule) => schedule.Precision switch
    {
        SchedulePrecision.DateAndTime or SchedulePrecision.DateOnly => Format(schedule.Date),
        SchedulePrecision.CompetitionPeriod => Format(schedule.PeriodStart),
        _ => "a-confirmar",
    };

    private static string Format(DateOnly? date) =>
        date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "a-confirmar";
}
