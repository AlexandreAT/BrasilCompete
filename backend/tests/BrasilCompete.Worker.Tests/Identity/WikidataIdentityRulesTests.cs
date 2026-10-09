using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

namespace BrasilCompete.Worker.Tests.Identity;

public sealed class WikidataIdentityRulesTests
{
    [Fact]
    public void Apply_RepresentsBrazilInWikidata_WhenTheSourceGivesNoCountry()
    {
        var result = Apply(Athlete(country: null), Record(representsBrazil: true));

        Assert.True(result.IsBrazilian);
        Assert.Equal(BrazilianReason.RepresentsBrazil, result.BrazilianReason);
        Assert.Equal(IdentityLayer.Wikidata, result.DecidedBy);
        Assert.Equal("Q1", result.ExternalIds[ExternalIdKeys.Wikidata]);
    }

    [Fact]
    public void Apply_BornInBrazilButSourceSaysAnotherCountry_IsBornInBrazil()
    {
        var result = Apply(Athlete(country: "POR"), Record(bornInBrazil: true, citizen: true));

        Assert.True(result.IsBrazilian);
        Assert.Equal(BrazilianReason.BornInBrazil, result.BrazilianReason);
    }

    [Fact]
    public void Apply_BornInBrazilAndCitizenWithoutSportCountry_IsInferredAsRepresentativeWithMediumConfidence()
    {
        var result = Apply(Athlete(country: null), Record(bornInBrazil: true, citizen: true));

        Assert.Equal(BrazilianReason.RepresentsBrazil, result.BrazilianReason);
        Assert.Equal(Confidence.Medium, result.IdentityConfidence);
    }

    [Fact]
    public void Apply_BornInBrazilButRepresentsAnotherCountry_IsBornInBrazil()
    {
        var result = Apply(Athlete(country: null), Record(bornInBrazil: true, citizen: true) with { RepresentsOtherCountries = ["Q45"] });

        Assert.Equal(BrazilianReason.BornInBrazil, result.BrazilianReason);
    }

    [Fact]
    public void Apply_CitizenshipOnly_IsNotIncludedButIsMeasured()
    {
        var result = Apply(Athlete(country: null), Record(citizen: true));

        Assert.False(result.IsBrazilian);
        Assert.True(result.BrazilianCitizenshipOnly);
    }

    [Fact]
    public void Apply_NameOnlyMatch_KeepsLowConfidence()
    {
        var result = WikidataIdentityRules.Apply(Athlete(country: null), new IdentityMatch(Record(representsBrazil: true), Confidence.Low));

        Assert.Equal(Confidence.Low, result.IdentityConfidence);
    }

    [Fact]
    public void Apply_NeverOverridesTheManualDecision()
    {
        var manual = Athlete(country: null) with { IsBrazilian = false, DecidedBy = IdentityLayer.Manual };

        Assert.False(Apply(manual, Record(representsBrazil: true)).IsBrazilian);
    }

    private static Participant Apply(Participant participant, IdentityRecord record) =>
        WikidataIdentityRules.Apply(participant, new IdentityMatch(record, Confidence.High));

    private static Participant Athlete(string? country) => new()
    {
        Name = "Atleta",
        Kind = ParticipantKind.Athlete,
        Country = country,
    };

    private static IdentityRecord Record(bool representsBrazil = false, bool bornInBrazil = false, bool citizen = false) => new()
    {
        WikidataId = "Q1",
        ProfileKey = "tennis",
        Sports = [Sport.Tennis],
        NameEn = "Atleta",
        RepresentsBrazil = representsBrazil,
        BornInBrazil = bornInBrazil,
        BrazilianCitizen = citizen,
    };
}
