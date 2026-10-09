using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

namespace BrasilCompete.Worker.Tests.Identity;

public sealed class IdentityIndexTests
{
    private static readonly IdentityRecord Supi = new()
    {
        WikidataId = "Q100",
        ProfileKey = "chess",
        Sports = [Sport.Chess],
        NameEn = "Luis Paulo Supi",
        EnglishWikipediaTitle = "Luis Paulo Supi",
        RepresentsBrazil = true,
        ExternalIds = new Dictionary<string, IReadOnlyList<string>> { ["fide"] = ["2112500"] },
    };

    private readonly IdentityIndex index = new([Supi]);

    [Fact]
    public void Find_ByExternalId_HasHighConfidence()
    {
        var match = index.Find(Participant("Supi, Luis Paulo", ("fide", "2112500")), Sport.Chess);

        Assert.Equal("Q100", match!.Record.WikidataId);
        Assert.Equal(Confidence.High, match.Confidence);
    }

    [Fact]
    public void Find_ByWikipediaTitleWithUnderscores_HasHighConfidence()
    {
        var match = index.Find(Participant("L. P. Supi", (ExternalIdKeys.EnglishWikipedia, "Luis_Paulo_Supi")), Sport.Chess);

        Assert.Equal(Confidence.High, match!.Confidence);
    }

    [Fact]
    public void Find_ByNameOnlyWithinTheSport_HasLowConfidence()
    {
        Assert.Equal(Confidence.Low, index.Find(Participant("Luís Paulo Supi"), Sport.Chess)!.Confidence);
        Assert.Null(index.Find(Participant("Luís Paulo Supi"), Sport.Tennis));
    }

    private static Participant Participant(string name, params (string Key, string Value)[] ids) => new()
    {
        Name = name,
        Kind = ParticipantKind.Athlete,
        ExternalIds = ids.ToDictionary(id => id.Key, id => id.Value),
    };
}
