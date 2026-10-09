using System.Globalization;

using BrasilCompete.Worker.Identity.Wikidata.Contracts;

namespace BrasilCompete.Worker.Identity.Wikidata;

public static class WikidataBindingMapper
{
    private const string EntityPrefix = "http://www.wikidata.org/entity/";

    public static string ToItemId(string uri) =>
        uri.StartsWith(EntityPrefix, StringComparison.Ordinal) ? uri[EntityPrefix.Length..] : uri;

    public static IReadOnlyList<string> ReadItemIds(SparqlResponse response) =>
        response.Results.Bindings
            .Where(binding => binding.ContainsKey("p"))
            .Select(binding => ToItemId(binding["p"].Value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

    public static IdentityRecord ToRecord(WikidataSportProfile profile, Dictionary<string, SparqlValue> binding)
    {
        var forSports = Split(binding, "forSports").Select(ToItemId).ToList();
        var externalIds = profile.ExternalIdProperties.Keys
            .Select(key => (Key: key, Values: Split(binding, $"ext_{WikidataQueryBuilder.Variable(key)}")))
            .Where(item => item.Values.Count > 0)
            .ToDictionary(item => item.Key, item => item.Values);

        return new IdentityRecord
        {
            WikidataId = ToItemId(binding["p"].Value),
            ProfileKey = profile.Key,
            Sports = profile.Sports,
            NameEn = Read(binding, "nameEn"),
            NamePt = Read(binding, "namePt"),
            EnglishWikipediaTitle = Read(binding, "enwiki"),
            BirthYear = ReadYear(binding, "birthDate"),
            RepresentsBrazil = forSports.Contains(WikidataSportProfiles.Brazil),
            RepresentsOtherCountries = forSports.Where(country => country != WikidataSportProfiles.Brazil).ToList(),
            BornInBrazil = Split(binding, "birthCountries").Select(ToItemId).Contains(WikidataSportProfiles.Brazil),
            BrazilianCitizen = Split(binding, "citizenships").Select(ToItemId).Contains(WikidataSportProfiles.Brazil),
            ExternalIds = externalIds,
        };
    }

    private static string? Read(Dictionary<string, SparqlValue> binding, string name) =>
        binding.TryGetValue(name, out var value) && value.Value.Length > 0 ? value.Value : null;

    private static IReadOnlyList<string> Split(Dictionary<string, SparqlValue> binding, string name) =>
        Read(binding, name)?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? [];

    private static int? ReadYear(Dictionary<string, SparqlValue> binding, string name) =>
        Read(binding, name) is { Length: >= 4 } value
            && int.TryParse(value.AsSpan(value[0] == '-' ? 1 : 0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                ? year
                : null;
}
