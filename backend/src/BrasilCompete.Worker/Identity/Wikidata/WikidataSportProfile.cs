using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>
/// Como encontrar os atletas de uma modalidade no Wikidata: pela modalidade (P641) ou pela ocupação (P106),
/// e quais IDs externos ligam o atleta às fontes.
/// </summary>
public sealed record WikidataSportProfile(
    string Key,
    IReadOnlyList<Sport> Sports,
    string SportItem,
    string OccupationItem,
    int? MinBirthYear,
    IReadOnlyDictionary<string, string> ExternalIdProperties);
