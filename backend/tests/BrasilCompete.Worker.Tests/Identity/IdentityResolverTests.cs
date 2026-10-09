using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

using static BrasilCompete.Worker.Tests.TestEvents;

namespace BrasilCompete.Worker.Tests.Identity;

public sealed class IdentityResolverTests
{
    private readonly IdentityResolver resolver = new();

    [Theory]
    [InlineData(ParticipantKind.Athlete, BrazilianReason.RepresentsBrazil)]
    [InlineData(ParticipantKind.NationalTeam, BrazilianReason.BrazilianNationalTeam)]
    [InlineData(ParticipantKind.Club, BrazilianReason.BrazilianClub)]
    [InlineData(ParticipantKind.Organization, BrazilianReason.BrazilianOrganization)]
    public void Resolve_SourceCountryBrazil_MarksAsBrazilianWithReasonByKind(ParticipantKind kind, BrazilianReason expected)
    {
        var participant = new Participant { Name = "Teste", Kind = kind, Country = "BRA" };
        var resolved = resolver.Resolve(Matchup(Sport.Football, Schedule.ToBeConfirmed(), "fonte", participant)).Participants[0];

        Assert.True(resolved.IsBrazilian);
        Assert.Equal(expected, resolved.BrazilianReason);
        Assert.Equal(IdentityLayer.Source, resolved.DecidedBy);
    }

    [Fact]
    public void Resolve_KeepsTheManualDecision()
    {
        var participant = new Participant
        {
            Name = "Atleta",
            Kind = ParticipantKind.Athlete,
            Country = "BRA",
            IsBrazilian = false,
            DecidedBy = IdentityLayer.Manual,
        };

        var resolved = resolver.Resolve(Matchup(Sport.Tennis, Schedule.ToBeConfirmed(), "manual", participant)).Participants[0];

        Assert.False(resolved.IsBrazilian);
        Assert.Equal(IdentityLayer.Manual, resolved.DecidedBy);
    }

    [Fact]
    public void Resolve_PairOfBrazilians_IsBrazilian()
    {
        var pair = Team("Dupla", ParticipantKind.Pair, "BRA") with { Country = null };
        pair = pair with { Members = [Athlete("Atleta 1", "BRA"), Athlete("Atleta 2", "BRA")] };

        var resolved = resolver.Resolve(Matchup(Sport.BeachVolleyball, Schedule.ToBeConfirmed(), "fonte", pair)).Participants[0];

        Assert.True(resolved.IsBrazilian);
        Assert.All(resolved.Members, member => Assert.True(member.IsBrazilian));
    }

    [Fact]
    public void HasBrazilian_FindsBrazilianMembersOfForeignTeams()
    {
        var club = Team("Clube Estrangeiro", ParticipantKind.Club, "ESP", Athlete("Jogador", "BRA"));
        var resolved = resolver.Resolve(Matchup(Sport.Football, Schedule.ToBeConfirmed(), "fonte", club));

        Assert.True(IdentityResolver.HasBrazilian(resolved));
        Assert.False(resolved.Participants[0].IsBrazilian);
    }
}
