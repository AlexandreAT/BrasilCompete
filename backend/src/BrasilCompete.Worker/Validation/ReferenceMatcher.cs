using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Validation;

/// <summary>
/// Procura, para cada linha do gabarito, o evento gerado da mesma modalidade, com data compatível (até um dia de
/// diferença, ou dentro do período) e com todos os participantes do gabarito. Nomes são comparados sem acentos,
/// espaços e maiúsculas, e um contido no outro basta ("Calderano" e "Hugo Calderano"). Nomes de países em inglês
/// das fontes ("Brazil") viram o nome em português ("Brasil").
/// </summary>
public static class ReferenceMatcher
{
    private const int MinimumContainedLength = 4;

    private static readonly TimeSpan TimeTolerance = TimeSpan.FromMinutes(5);

    public static IReadOnlyList<ReferenceMatch> Match(IReadOnlyList<ReferenceRow> rows, IReadOnlyList<SportEvent> events) =>
        rows.Where(row => row.Date is not null).Select(row => Match(row, events)).ToList();

    public static ReferenceMatch Match(ReferenceRow row, IReadOnlyList<SportEvent> events)
    {
        var candidate = events
            .Where(sportEvent => string.Equals(sportEvent.Sport.ToSlug(), row.Sport, StringComparison.OrdinalIgnoreCase))
            .Select(sportEvent => (Event: sportEvent, Distance: DateDistance(row, sportEvent)))
            .Where(item => item.Distance is <= 1 && HasAllParticipants(row, item.Event))
            .OrderBy(item => item.Distance)
            .ThenByDescending(item => StageMatches(row, item.Event))
            .Select(item => item.Event)
            .FirstOrDefault();

        if (candidate is null)
        {
            return new ReferenceMatch(row, null, null, null);
        }

        var dateCorrect = candidate.Schedule.Date is { } date ? date == row.Date : (bool?)null;
        var timeCorrect = row.Time is { } time && candidate.Schedule is { Precision: SchedulePrecision.DateAndTime, StartUtc: { } start }
            ? (TimeOnly.FromDateTime(BrasiliaTime.ToLocal(start)) - time).Duration() <= TimeTolerance
                && BrasiliaTime.ToDate(start) == row.Date
            : (bool?)null;

        return new ReferenceMatch(row, candidate, dateCorrect, timeCorrect);
    }

    /// <summary>Dias entre o gabarito e o evento; 0 quando a data do gabarito cai no período do evento.</summary>
    private static int? DateDistance(ReferenceRow row, SportEvent sportEvent)
    {
        if (row.Date is not { } date)
        {
            return null;
        }

        return sportEvent.Schedule switch
        {
            { Date: { } day } => Math.Abs(day.DayNumber - date.DayNumber),
            { PeriodStart: { } start, PeriodEnd: { } end } => date >= start && date <= end ? 0 : null,
            _ => null,
        };
    }

    private static bool HasAllParticipants(ReferenceRow row, SportEvent sportEvent)
    {
        var names = sportEvent.Participants
            .SelectMany(participant => new[] { participant.Name, participant.Team }.Concat(participant.Members.Select(member => member.Name)))
            .OfType<string>()
            .Select(ToPortugueseCountryName)
            .Select(TextNormalizer.NormalizeName)
            .Where(name => name.Length > 0)
            .ToList();

        return row.Participants.Count > 0
            && row.Participants.Select(TextNormalizer.NormalizeName).All(expected => names.Any(name => SameName(expected, name)));
    }

    private static bool StageMatches(ReferenceRow row, SportEvent sportEvent) =>
        sportEvent.Stage is { } stage && TextNormalizer.NormalizeName(row.Stage).Contains(TextNormalizer.NormalizeName(stage), StringComparison.Ordinal);

    private static bool SameName(string expected, string actual) =>
        expected == actual
        || (Math.Min(expected.Length, actual.Length) >= MinimumContainedLength
            && (expected.Contains(actual, StringComparison.Ordinal) || actual.Contains(expected, StringComparison.Ordinal)));

    private static string ToPortugueseCountryName(string name) =>
        CountryCodes.FromEnglishName(name) is { } code ? CountryNames.For(code) : name;
}
