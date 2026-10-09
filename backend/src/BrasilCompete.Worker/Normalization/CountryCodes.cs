using System.Collections.Frozen;

namespace BrasilCompete.Worker.Normalization;

/// <summary>
/// Converte nacionalidades (adjetivos em inglês, como "Brazilian") em códigos ISO 3166-1 alfa-3.
/// Para a identidade, só importa distinguir o Brasil; os demais países servem para a agenda e as métricas.
/// </summary>
public static class CountryCodes
{
    public const string Brazil = "BRA";

    private static readonly FrozenDictionary<string, string> ByDemonym = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["American"] = "USA",
        ["Argentine"] = "ARG",
        ["Argentinian"] = "ARG",
        ["Australian"] = "AUS",
        ["Austrian"] = "AUT",
        ["Belgian"] = "BEL",
        ["Brazilian"] = Brazil,
        ["British"] = "GBR",
        ["Canadian"] = "CAN",
        ["Chilean"] = "CHL",
        ["Chinese"] = "CHN",
        ["Colombian"] = "COL",
        ["Czech"] = "CZE",
        ["Danish"] = "DNK",
        ["Dutch"] = "NLD",
        ["Emirati"] = "ARE",
        ["Estonian"] = "EST",
        ["Finnish"] = "FIN",
        ["French"] = "FRA",
        ["German"] = "DEU",
        ["Hungarian"] = "HUN",
        ["Indian"] = "IND",
        ["Indonesian"] = "IDN",
        ["Irish"] = "IRL",
        ["Israeli"] = "ISR",
        ["Italian"] = "ITA",
        ["Japanese"] = "JPN",
        ["Malaysian"] = "MYS",
        ["Mexican"] = "MEX",
        ["Monegasque"] = "MCO",
        ["New Zealander"] = "NZL",
        ["Polish"] = "POL",
        ["Portuguese"] = "PRT",
        ["Russian"] = "RUS",
        ["South African"] = "ZAF",
        ["Spanish"] = "ESP",
        ["Swedish"] = "SWE",
        ["Swiss"] = "CHE",
        ["Thai"] = "THA",
        ["Uruguayan"] = "URY",
        ["Venezuelan"] = "VEN",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, string> ByEnglishName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Argentina"] = "ARG",
        ["Australia"] = "AUS",
        ["Brazil"] = Brazil,
        ["Canada"] = "CAN",
        ["Chile"] = "CHL",
        ["China"] = "CHN",
        ["Denmark"] = "DNK",
        ["Finland"] = "FIN",
        ["France"] = "FRA",
        ["Germany"] = "DEU",
        ["Japan"] = "JPN",
        ["Korea"] = "KOR",
        ["South Korea"] = "KOR",
        ["Mexico"] = "MEX",
        ["Netherlands"] = "NLD",
        ["Peru"] = "PER",
        ["Poland"] = "POL",
        ["Portugal"] = "PRT",
        ["Russia"] = "RUS",
        ["Spain"] = "ESP",
        ["Sweden"] = "SWE",
        ["Turkey"] = "TUR",
        ["Ukraine"] = "UKR",
        ["United Kingdom"] = "GBR",
        ["United States"] = "USA",
        ["Vietnam"] = "VNM",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Código do país pelo nome em inglês ("Brazil"). Desconhecido volta em maiúsculas.</summary>
    public static string? FromEnglishName(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : ByEnglishName.GetValueOrDefault(name.Trim()) ?? name.Trim().ToUpperInvariant();

    /// <summary>
    /// Código do país pela nacionalidade. Uma nacionalidade desconhecida volta como texto em maiúsculas, para
    /// continuar valendo como "outro país" na identidade.
    /// </summary>
    public static string? FromDemonym(string? demonym) =>
        string.IsNullOrWhiteSpace(demonym)
            ? null
            : ByDemonym.GetValueOrDefault(demonym.Trim()) ?? demonym.Trim().ToUpperInvariant();
}
