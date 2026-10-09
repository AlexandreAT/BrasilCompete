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

    /// <summary>
    /// Primeiro com todos os participantes. Sem isso, aceita o evento em que só parte dos nomes bate, desde que ele
    /// seja o único candidato no dia mais próximo (um lutador luta uma vez por evento; no tênis de mesa, com vários
    /// jogos no mesmo período, o adversário continua obrigatório).
    /// </summary>
    public static ReferenceMatch Match(ReferenceRow row, IReadOnlyList<SportEvent> events)
    {
        var candidates = events
            .Where(sportEvent => string.Equals(sportEvent.Sport.ToSlug(), row.Sport, StringComparison.OrdinalIgnoreCase))
            .Select(sportEvent => (Event: sportEvent, Distance: DateDistance(row, sportEvent), Matched: MatchedParticipants(row, sportEvent)))
            .Where(item => item.Distance is <= 1 && item.Matched > 0)
            .ToList();

        var candidate = candidates
            .Where(item => item.Matched == row.Participants.Count)
            .OrderBy(item => item.Distance)
            .ThenByDescending(item => StageMatches(row, item.Event))
            .Select(item => item.Event)
            .FirstOrDefault();
        var nameMismatch = false;

        if (candidate is null && candidates.Count > 0)
        {
            var closest = candidates.Where(item => item.Distance == candidates.Min(other => other.Distance)).ToList();

            if (closest.Count == 1)
            {
                candidate = closest[0].Event;
                nameMismatch = true;
            }
        }

        if (candidate is null)
        {
            return new ReferenceMatch(row, null, null, null);
        }

        var dateCorrect = candidate.Schedule.Date is { } date ? date == row.Date : (bool?)null;
        var timeCorrect = row.Time is { } time && candidate.Schedule is { Precision: SchedulePrecision.DateAndTime, StartUtc: { } start }
            ? (TimeOnly.FromDateTime(BrasiliaTime.ToLocal(start)) - time).Duration() <= TimeTolerance
                && BrasiliaTime.ToDate(start) == row.Date
            : (bool?)null;

        return new ReferenceMatch(row, candidate, dateCorrect, timeCorrect, nameMismatch);
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

    private static int MatchedParticipants(ReferenceRow row, SportEvent sportEvent)
    {
        var names = sportEvent.Participants
            .SelectMany(participant => new[] { participant.Name, participant.Team }.Concat(participant.Members.Select(member => member.Name)))
            .OfType<string>()
            .Select(ToPortugueseCountryName)
            .Select(TextNormalizer.NormalizeName)
            .Where(name => name.Length > 0)
            .ToList();

        return row.Participants.Select(TextNormalizer.NormalizeName).Count(expected => names.Any(name => SameName(expected, name)));
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
