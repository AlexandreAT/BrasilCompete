using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

namespace BrasilCompete.Worker.Output;

public static class RunTotalsFactory
{
    public static RunTotals Create(PipelineResult result)
    {
        var participants = result.Events
            .Concat(result.Discarded.Select(item => item.Event))
            .SelectMany(sportEvent => sportEvent.Participants.SelectMany(participant => participant.Members.Prepend(participant)))
            .ToList();

        return new RunTotals(
            result.CollectedCount,
            result.Events.Count,
            result.Events.Count(sportEvent => sportEvent.View == EventView.Main),
            result.Events.Count(sportEvent => sportEvent.View == EventView.Individuals),
            Enum.GetValues<DiscardReason>().ToDictionary(
                reason => reason,
                reason => result.Discarded.Count(item => item.Reason == reason)),
            result.MergedCount,
            result.Merges.Count(merge => merge.AcrossSources),
            result.Conflicts.Count,
            new IdentityTotals(
                participants.Count(participant => participant.IsBrazilian && participant.DecidedBy == IdentityLayer.Source),
                participants.Count(participant => participant.IsBrazilian && participant.DecidedBy == IdentityLayer.Wikidata),
                participants.Count(participant => participant.IsBrazilian && participant.DecidedBy == IdentityLayer.Manual),
                participants.Count(participant => participant.IsBrazilian && participant.IdentityConfidence == Confidence.Low),
                participants.Count(participant => participant.BrazilianCitizenshipOnly)));
    }
}
