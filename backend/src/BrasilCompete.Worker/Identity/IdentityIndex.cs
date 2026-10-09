using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Identity;

/// <summary>
/// Busca rápida no catálogo: primeiro por ID externo, depois por ID do Wikidata ou artigo da Wikipedia
/// e, por último, pelo nome dentro da modalidade (baixa confiança, e só quando o nome é único).
/// </summary>
public sealed class IdentityIndex
{
    private readonly Dictionary<string, IdentityRecord> byExternalId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IdentityRecord> byWikidataId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IdentityRecord> byWikipediaTitle = new(StringComparer.Ordinal);
    private readonly Dictionary<(Sport Sport, string Name), List<IdentityRecord>> byName = [];

    public IdentityIndex(IEnumerable<IdentityRecord> records)
    {
        foreach (var record in records)
        {
            Count++;
            byWikidataId.TryAdd(record.WikidataId, record);

            if (record.EnglishWikipediaTitle is { } title)
            {
                byWikipediaTitle.TryAdd(NormalizeTitle(title), record);
            }

            foreach (var (key, values) in record.ExternalIds)
            {
                foreach (var value in values)
                {
                    byExternalId.TryAdd(ExternalKey(key, value), record);
                }
            }

            foreach (var sport in record.Sports)
            {
                foreach (var name in new[] { record.NameEn, record.NamePt }.OfType<string>().Distinct())
                {
                    var key = (sport, TextNormalizer.NormalizeName(name));

                    if (!byName.TryGetValue(key, out var list))
                    {
                        byName[key] = list = [];
                    }

                    if (!list.Contains(record))
                    {
                        list.Add(record);
                    }
                }
            }
        }
    }

    public static IdentityIndex Empty { get; } = new([]);

    public int Count { get; }

    public IdentityMatch? Find(Participant participant, Sport sport)
    {
        foreach (var (key, value) in participant.ExternalIds)
        {
            var record = key switch
            {
                ExternalIdKeys.Wikidata => byWikidataId.GetValueOrDefault(value),
                ExternalIdKeys.EnglishWikipedia => byWikipediaTitle.GetValueOrDefault(NormalizeTitle(value)),
                _ => byExternalId.GetValueOrDefault(ExternalKey(key, value)),
            };

            if (record is not null)
            {
                return new IdentityMatch(record, Confidence.High);
            }
        }

        return byName.TryGetValue((sport, TextNormalizer.NormalizeName(participant.Name)), out var candidates) && candidates.Count == 1
            ? new IdentityMatch(candidates[0], Confidence.Low)
            : null;
    }

    private static string ExternalKey(string key, string value) => $"{key}:{value.Trim()}";

    private static string NormalizeTitle(string title) => title.Replace('_', ' ').Trim();
}
