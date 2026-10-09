using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity;

/// <summary>Um atleta ligado ao Brasil segundo o Wikidata, com os IDs que o ligam às fontes.</summary>
public sealed record IdentityRecord
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> NoExternalIds =
        new Dictionary<string, IReadOnlyList<string>>();

    public required string WikidataId { get; init; }

    public required string ProfileKey { get; init; }

    public required IReadOnlyList<Sport> Sports { get; init; }

    public string? NameEn { get; init; }

    public string? NamePt { get; init; }

    public string? EnglishWikipediaTitle { get; init; }

    public int? BirthYear { get; init; }

    public bool RepresentsBrazil { get; init; }

    /// <summary>Outros países pelos quais o atleta compete (P1532), em IDs do Wikidata.</summary>
    public IReadOnlyList<string> RepresentsOtherCountries { get; init; } = [];

    public bool BornInBrazil { get; init; }

    public bool BrazilianCitizen { get; init; }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ExternalIds { get; init; } = NoExternalIds;

    /// <summary>Só a cidadania liga o atleta ao Brasil: não entra, mas é medido (plano, seção 4.1).</summary>
    public bool IsCitizenshipOnly => BrazilianCitizen && !RepresentsBrazil && !BornInBrazil;
}
