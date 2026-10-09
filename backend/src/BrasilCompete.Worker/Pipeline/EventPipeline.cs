using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Identidade, filtro, classificação e deduplicação (plano, seção 9.3).
/// Os eventos descartados são guardados com o motivo, para as métricas de falso positivo e de volume.
/// </summary>
public sealed class EventPipeline(IdentityResolver identity, ViewClassifier classifier, EventDeduplicator deduplicator)
{
    public PipelineResult Run(IReadOnlyList<SportEvent> collected, DateWindow window)
    {
        var accepted = new List<SportEvent>();
        var discarded = new List<DiscardedEvent>();

        foreach (var raw in collected)
        {
            var resolved = identity.Resolve(raw);
            var withId = resolved with { Id = EventIdFactory.Create(resolved) };
            var view = classifier.Classify(withId);
            var reason = GetDiscardReason(withId, view, window);

            if (reason is null)
            {
                accepted.Add(withId with { View = view });
            }
            else
            {
                discarded.Add(new DiscardedEvent(withId, reason.Value));
            }
        }

        var deduplicated = deduplicator.Merge(accepted);
        var ordered = deduplicated.Events.OrderBy(SortKey).ThenBy(sportEvent => sportEvent.Id, StringComparer.Ordinal).ToList();

        return new PipelineResult(collected.Count, ordered, discarded, deduplicated.Conflicts, deduplicated.MergedCount);
    }

    private static DiscardReason? GetDiscardReason(SportEvent sportEvent, EventView? view, DateWindow window)
    {
        if (!IdentityResolver.HasBrazilian(sportEvent))
        {
            return DiscardReason.NoBrazilian;
        }

        if (view is null)
        {
            return DiscardReason.NotInternational;
        }

        return window.Overlaps(sportEvent.Schedule) ? null : DiscardReason.OutOfWindow;
    }

    private static DateTimeOffset SortKey(SportEvent sportEvent)
    {
        var schedule = sportEvent.Schedule;

        if (schedule.StartUtc is { } start)
        {
            return start;
        }

        var date = schedule.Date ?? schedule.PeriodStart;

        return date is { } day
            ? new DateTimeOffset(day.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero)
            : DateTimeOffset.MaxValue;
    }
}
