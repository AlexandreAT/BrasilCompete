using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>
/// País de cada lutador do elenco atual do UFC, pela bandeira. Busca pelo artigo da Wikipedia e, para quem não tem
/// artigo (muitos lutadores novos), pelo nome normalizado na célula do nome.
/// </summary>
public sealed class UfcRoster
{
    private readonly Dictionary<string, string> byTitle = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> byName = new(StringComparer.Ordinal);

    public static UfcRoster Empty { get; } = new();

    public int Count => byName.Count;

    public void Add(string name, string? wikipediaTitle, string country)
    {
        byName.TryAdd(TextNormalizer.NormalizeName(name), country);

        if (wikipediaTitle is not null)
        {
            byTitle.TryAdd(wikipediaTitle, country);
        }
    }

    public string? CountryOf(UfcFighter fighter) =>
        fighter.WikipediaTitle is { } title && byTitle.TryGetValue(title, out var country)
            ? country
            : byName.GetValueOrDefault(TextNormalizer.NormalizeName(fighter.Name));
}
