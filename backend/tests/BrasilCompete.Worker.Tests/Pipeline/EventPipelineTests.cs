using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Options;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Pipeline;

public sealed class EventPipelineTests
{
    private static readonly DateWindow October = new(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

    private readonly EventPipeline pipeline = new(
        new IdentityResolver(),
        new ViewClassifier(),
        new EventDeduplicator(new SourcePriority(Options.Create(new WorkerOptions()))));

    [Fact]
    public void Run_KeepsDiscardedEventsWithTheReason()
    {
        var noBrazilian = Matchup(Sport.Football, At(2026, 10, 10, 18), "fonte", Team("A", ParticipantKind.Club, "ESP"), Team("B", ParticipantKind.Club, "ITA"));
        var domestic = Matchup(Sport.Football, At(2026, 10, 11, 18), "fonte", Team("C", ParticipantKind.Club, "BRA"), Team("D", ParticipantKind.Club, "BRA")) with
        {
            Scope = CompetitionScope.BrazilianDomestic,
        };
        var outside = Participation(Sport.Formula1, At(2026, 11, 8, 17), "fonte", "Corrida", Athlete("Piloto", "BRA"));
        var accepted = Participation(Sport.Formula1, At(2026, 10, 25, 19), "fonte", "Corrida", Athlete("Piloto", "BRA"));

        var result = pipeline.Run([noBrazilian, domestic, outside, accepted], October);

        var sportEvent = Assert.Single(result.Events);
        Assert.Equal(EventView.Main, sportEvent.View);
        Assert.NotEmpty(sportEvent.Id);
        Assert.Equal(
            [DiscardReason.NoBrazilian, DiscardReason.NotInternational, DiscardReason.OutOfWindow],
            result.Discarded.Select(item => item.Reason));
    }

    [Fact]
    public void Run_OrdersEventsByTime()
    {
        var later = Participation(Sport.Formula1, At(2026, 10, 25, 19), "fonte", "Corrida", Athlete("Piloto", "BRA"));
        var earlier = Participation(Sport.Formula1, At(2026, 10, 24, 19), "fonte", "Classificação", Athlete("Piloto", "BRA"));

        var result = pipeline.Run([later, earlier], October);

        Assert.Equal(["Classificação", "Corrida"], result.Events.Select(sportEvent => sportEvent.Stage));
    }
}
