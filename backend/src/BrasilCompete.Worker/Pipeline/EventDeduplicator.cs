using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Manual;

namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Junta o mesmo evento vindo de fontes diferentes (plano, seção 9.7): mesma modalidade e formato,
/// mesmos participantes e datas com até um dia de diferença (por causa de fuso).
/// </summary>
public sealed class EventDeduplicator(SourcePriority priority)
{
    private const int DateToleranceInDays = 1;

    public DeduplicationResult Merge(IReadOnlyList<SportEvent> events)
    {
        var groups = new List<List<SportEvent>>();

        foreach (var sportEvent in events.OrderBy(priority.Rank))
        {
            var group = groups.Find(candidate => IsSameEvent(candidate[0], sportEvent));

            if (group is null)
            {
                groups.Add([sportEvent]);
            }
            else
            {
                group.Add(sportEvent);
            }
        }

        var merged = new List<SportEvent>(groups.Count);
        var conflicts = new List<MergeConflict>();
        var merges = new List<MergedEvent>();

        foreach (var group in groups)
        {
            var result = group.Count == 1 ? group[0] : MergeGroup(group);
            merged.Add(result);
            conflicts.AddRange(FindConflicts(result.Id, group));

            if (group.Count > 1)
            {
                var sources = group.Select(sportEvent => $"{SourceOf(sportEvent)}:{sportEvent.Sources[0].ExternalId}").ToList();
                merges.Add(new MergedEvent(result.Id, sources, group.Select(SourceOf).Distinct().Count() > 1));
            }
        }

        return new DeduplicationResult(merged, conflicts, merges);
    }

    public static bool IsSameEvent(SportEvent first, SportEvent second) =>
        first.Sport == second.Sport
        && first.Format == second.Format
        && !AreDistinctInSameSource(first, second)
        && HasCompatibleStage(first, second)
        && ParticipantKeys.For(first).SetEquals(ParticipantKeys.For(second))
        && AreDatesClose(first.Schedule, second.Schedule);

    private SportEvent MergeGroup(List<SportEvent> group)
    {
        var primary = group[0];
        var schedule = IsManual(primary)
            ? primary.Schedule
            : group.OrderBy(sportEvent => sportEvent.Schedule.Precision).ThenBy(priority.Rank).First().Schedule;

        var merged = primary with
        {
            Schedule = schedule,
            Sources = group.SelectMany(sportEvent => sportEvent.Sources).Distinct().ToList(),
            Confidence = group.Min(sportEvent => sportEvent.Confidence),
        };

        return merged with { Id = EventIdFactory.Create(merged) };
    }

    private static IEnumerable<MergeConflict> FindConflicts(string eventId, List<SportEvent> group)
    {
        var times = ValuesBySource(group, sportEvent =>
            sportEvent.Schedule.StartUtc?.ToString("u", CultureInfo.InvariantCulture));

        if (times.Values.Distinct().Count() > 1)
        {
            yield return new MergeConflict(eventId, "startUtc", times);
        }

        var dates = ValuesBySource(group, sportEvent =>
            sportEvent.Schedule.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (dates.Values.Distinct().Count() > 1)
        {
            yield return new MergeConflict(eventId, "date", dates);
        }
    }

    private static Dictionary<string, string> ValuesBySource(List<SportEvent> group, Func<SportEvent, string?> selector) =>
        group
            .Select(sportEvent => (Source: SourceOf(sportEvent), Value: selector(sportEvent)))
            .Where(item => item.Value is not null)
            .GroupBy(item => item.Source)
            .ToDictionary(items => items.Key, items => items.First().Value!);

    private static bool AreDistinctInSameSource(SportEvent first, SportEvent second) =>
        first.Sources.Any(left => second.Sources.Any(right =>
            left.Source == right.Source && left.ExternalId != right.ExternalId));

    /// <summary>Participações (sessões da F1, rodadas de torneio) só se juntam com a mesma fase.</summary>
    private static bool HasCompatibleStage(SportEvent first, SportEvent second) =>
        first.Format == EventFormat.Matchup
        || first.Stage is null
        || second.Stage is null
        || string.Equals(first.Stage, second.Stage, StringComparison.OrdinalIgnoreCase);

    private static bool AreDatesClose(Schedule first, Schedule second)
    {
        var firstSpan = first.GetDateSpan();
        var secondSpan = second.GetDateSpan();

        if (firstSpan is null || secondSpan is null)
        {
            return true;
        }

        var difference = Math.Abs(firstSpan.Value.First.DayNumber - secondSpan.Value.First.DayNumber);

        return difference <= DateToleranceInDays;
    }

    private static bool IsManual(SportEvent sportEvent) =>
        sportEvent.Sources.Any(source => source.Source == ManualEventSource.SourceName);

    private static string SourceOf(SportEvent sportEvent) => sportEvent.Sources[0].Source;
}
