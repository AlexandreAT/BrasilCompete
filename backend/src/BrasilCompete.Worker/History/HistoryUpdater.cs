using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

namespace BrasilCompete.Worker.History;

/// <summary>
/// Compara a execução atual com o histórico. Quando o identificador muda (o evento ganhou data ou mudou de dia),
/// reconhece o evento pela chave sem data e registra a mudança, em vez de contá-lo como novo.
/// </summary>
public sealed class HistoryUpdater(EventHistoryStore store)
{
    public async Task<HistoryUpdateResult> UpdateAsync(
        IReadOnlyList<SportEvent> events,
        DateWindow window,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        var history = await store.LoadAsync(cancellationToken);
        var result = Apply(history, events, window, runAtUtc);
        await store.SaveAsync(history, cancellationToken);

        return result;
    }

    public static HistoryUpdateResult Apply(
        EventHistoryFile history,
        IReadOnlyList<SportEvent> events,
        DateWindow window,
        DateTimeOffset runAtUtc)
    {
        var entries = history.Events;
        var currentIds = events.Select(sportEvent => sportEvent.Id).ToHashSet(StringComparer.Ordinal);
        int created = 0, changed = 0, unchanged = 0;

        foreach (var sportEvent in events)
        {
            var snapshot = EventSnapshot.From(sportEvent);
            var matchKey = EventIdFactory.CreateMatchKey(sportEvent);
            var changes = new List<EventChange>();

            if (!entries.TryGetValue(sportEvent.Id, out var entry))
            {
                var previousId = FindRenamedEntry(entries, currentIds, matchKey, snapshot);

                if (previousId is null)
                {
                    entries[sportEvent.Id] = NewEntry(matchKey, snapshot, runAtUtc);
                    created++;
                    continue;
                }

                entry = entries[previousId];
                entries.Remove(previousId);
                changes.Add(new EventChange(runAtUtc, "id", previousId, sportEvent.Id));
            }

            changes.AddRange(entry.Snapshot.Diff(snapshot).Select(diff => new EventChange(runAtUtc, diff.Field, diff.From, diff.To)));

            entries[sportEvent.Id] = entry with
            {
                MatchKey = matchKey,
                FirstSeenWithDateUtc = entry.FirstSeenWithDateUtc ?? (HasDate(snapshot) ? runAtUtc : null),
                LastSeenUtc = runAtUtc,
                MissingSinceUtc = null,
                Snapshot = snapshot,
                Changes = [.. entry.Changes, .. changes],
            };

            if (changes.Count > 0)
            {
                changed++;
            }
            else
            {
                unchanged++;
            }
        }

        var missing = MarkMissing(entries, currentIds, window, runAtUtc);

        return new HistoryUpdateResult(created, changed, unchanged, missing);
    }

    private static string? FindRenamedEntry(
        Dictionary<string, EventHistoryEntry> entries,
        HashSet<string> currentIds,
        string matchKey,
        EventSnapshot snapshot) =>
        entries
            .Where(pair => pair.Value.MatchKey == matchKey && !currentIds.Contains(pair.Key))
            .OrderBy(pair => DistanceInDays(pair.Value.Snapshot, snapshot))
            .Select(pair => pair.Key)
            .FirstOrDefault();

    private static int MarkMissing(
        Dictionary<string, EventHistoryEntry> entries,
        HashSet<string> currentIds,
        DateWindow window,
        DateTimeOffset runAtUtc)
    {
        var missing = 0;

        foreach (var (id, entry) in entries.ToList())
        {
            if (currentIds.Contains(id) || !IsInside(window, entry.Snapshot))
            {
                continue;
            }

            entries[id] = entry with { MissingSinceUtc = entry.MissingSinceUtc ?? runAtUtc };
            missing++;
        }

        return missing;
    }

    private static EventHistoryEntry NewEntry(string matchKey, EventSnapshot snapshot, DateTimeOffset runAtUtc) => new()
    {
        MatchKey = matchKey,
        FirstSeenUtc = runAtUtc,
        FirstSeenWithDateUtc = HasDate(snapshot) ? runAtUtc : null,
        LastSeenUtc = runAtUtc,
        Snapshot = snapshot,
    };

    private static bool HasDate(EventSnapshot snapshot) =>
        snapshot.Precision is SchedulePrecision.DateAndTime or SchedulePrecision.DateOnly;

    private static bool IsInside(DateWindow window, EventSnapshot snapshot) =>
        snapshot.ReferenceDate is not { } date || (date >= window.From && date <= window.To);

    private static int DistanceInDays(EventSnapshot previous, EventSnapshot current) =>
        previous.ReferenceDate is { } before && current.ReferenceDate is { } after
            ? Math.Abs(before.DayNumber - after.DayNumber)
            : int.MaxValue;
}
