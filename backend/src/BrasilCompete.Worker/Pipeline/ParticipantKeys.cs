using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Pipeline;

public static class ParticipantKeys
{
    public const string WikidataIdKey = "wikidata";

    /// <summary>Casa participantes pelo ID do Wikidata quando existir; senão, pelo nome normalizado.</summary>
    public static string For(Participant participant) =>
        participant.ExternalIds.TryGetValue(WikidataIdKey, out var wikidataId)
            ? $"wd:{wikidataId}"
            : $"name:{TextNormalizer.NormalizeName(participant.Name)}";

    public static IReadOnlySet<string> For(SportEvent sportEvent) =>
        sportEvent.Participants.Select(For).ToHashSet(StringComparer.Ordinal);
}
