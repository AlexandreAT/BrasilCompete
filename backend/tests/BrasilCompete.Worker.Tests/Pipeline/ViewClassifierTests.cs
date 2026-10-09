using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Pipeline;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Pipeline;

public sealed class ViewClassifierTests
{
    private readonly IdentityResolver resolver = new(IdentityIndex.Empty);
    private readonly ViewClassifier classifier = new();

    [Fact]
    public void Classify_BrazilianAthleteInIndividualSport_IsMain()
    {
        var sportEvent = Participation(Sport.Formula1, Schedule.ToBeConfirmed(), "fonte", "Corrida", Athlete("Piloto", "BRA"));

        Assert.Equal(EventView.Main, Classify(sportEvent));
    }

    [Fact]
    public void Classify_AthleteOnlyBornInBrazil_IsIndividuals()
    {
        var athlete = Athlete("Nascido no Brasil", "POR", BrazilianReason.BornInBrazil);
        var sportEvent = Matchup(Sport.Tennis, Schedule.ToBeConfirmed(), "fonte", athlete, Athlete("Rival", "ESP"));

        Assert.Equal(EventView.Individuals, Classify(sportEvent));
    }

    [Fact]
    public void Classify_BrazilianClubInInternationalCompetition_IsMain()
    {
        var sportEvent = Matchup(
            Sport.Football,
            Schedule.ToBeConfirmed(),
            "fonte",
            Team("Clube Brasileiro", ParticipantKind.Club, "BRA"),
            Team("Clube Argentino", ParticipantKind.Club, "ARG"));

        Assert.Equal(EventView.Main, Classify(sportEvent));
    }

    [Fact]
    public void Classify_BrazilianInForeignLeague_IsIndividuals()
    {
        var sportEvent = Matchup(
            Sport.Basketball,
            Schedule.ToBeConfirmed(),
            "fonte",
            Team("Time A", ParticipantKind.Club, "USA", Athlete("Brasileiro", "BRA")),
            Team("Time B", ParticipantKind.Club, "USA")) with { Scope = CompetitionScope.ForeignDomestic };

        Assert.Equal(EventView.Individuals, Classify(sportEvent));
    }

    [Fact]
    public void Classify_BrazilianDomesticLeague_IsNotIncluded()
    {
        var sportEvent = Matchup(
            Sport.EsportsLeagueOfLegends,
            Schedule.ToBeConfirmed(),
            "fonte",
            Team("Organização A", ParticipantKind.Organization, "BRA"),
            Team("Organização B", ParticipantKind.Organization, "BRA")) with { Scope = CompetitionScope.BrazilianDomestic };

        Assert.Null(Classify(sportEvent));
    }

    [Fact]
    public void Classify_ForeignTeamsWithoutBrazilians_IsNotIncluded()
    {
        var sportEvent = Matchup(
            Sport.Football,
            Schedule.ToBeConfirmed(),
            "fonte",
            Team("Clube A", ParticipantKind.Club, "ESP"),
            Team("Clube B", ParticipantKind.Club, "ITA"));

        Assert.Null(Classify(sportEvent));
    }

    private EventView? Classify(SportEvent sportEvent) => classifier.Classify(resolver.Resolve(sportEvent));
}
