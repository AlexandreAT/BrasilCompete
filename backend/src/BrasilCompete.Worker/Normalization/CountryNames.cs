using System.Collections.Frozen;

namespace BrasilCompete.Worker.Normalization;

/// <summary>
/// Nomes em português das seleções, pelo código de três letras usado nas fontes esportivas (FIFA/COI).
/// Um código desconhecido aparece como está.
/// </summary>
public static class CountryNames
{
    private static readonly FrozenDictionary<string, string> ByCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ALG"] = "Argélia",
        ["ARG"] = "Argentina",
        ["AUS"] = "Austrália",
        ["AUT"] = "Áustria",
        ["BEL"] = "Bélgica",
        ["BOL"] = "Bolívia",
        ["BRA"] = "Brasil",
        ["CAN"] = "Canadá",
        ["CHI"] = "Chile",
        ["CHN"] = "China",
        ["CIV"] = "Costa do Marfim",
        ["CMR"] = "Camarões",
        ["COL"] = "Colômbia",
        ["CRC"] = "Costa Rica",
        ["CRO"] = "Croácia",
        ["CZE"] = "Tchéquia",
        ["DEN"] = "Dinamarca",
        ["ECU"] = "Equador",
        ["EGY"] = "Egito",
        ["ENG"] = "Inglaterra",
        ["ESP"] = "Espanha",
        ["FIN"] = "Finlândia",
        ["FRA"] = "França",
        ["GER"] = "Alemanha",
        ["GHA"] = "Gana",
        ["GRE"] = "Grécia",
        ["HAI"] = "Haiti",
        ["HUN"] = "Hungria",
        ["IRL"] = "Irlanda",
        ["IRN"] = "Irã",
        ["ISL"] = "Islândia",
        ["ITA"] = "Itália",
        ["JAM"] = "Jamaica",
        ["JPN"] = "Japão",
        ["KOR"] = "Coreia do Sul",
        ["KSA"] = "Arábia Saudita",
        ["MAR"] = "Marrocos",
        ["MEX"] = "México",
        ["NED"] = "Holanda",
        ["NGA"] = "Nigéria",
        ["NIR"] = "Irlanda do Norte",
        ["NOR"] = "Noruega",
        ["NZL"] = "Nova Zelândia",
        ["PAN"] = "Panamá",
        ["PAR"] = "Paraguai",
        ["PER"] = "Peru",
        ["POL"] = "Polônia",
        ["POR"] = "Portugal",
        ["QAT"] = "Catar",
        ["ROU"] = "Romênia",
        ["RSA"] = "África do Sul",
        ["RUS"] = "Rússia",
        ["SCO"] = "Escócia",
        ["SEN"] = "Senegal",
        ["SRB"] = "Sérvia",
        ["SUI"] = "Suíça",
        ["SVK"] = "Eslováquia",
        ["SVN"] = "Eslovênia",
        ["SWE"] = "Suécia",
        ["TUN"] = "Tunísia",
        ["TUR"] = "Turquia",
        ["UKR"] = "Ucrânia",
        ["URU"] = "Uruguai",
        ["USA"] = "Estados Unidos",
        ["VEN"] = "Venezuela",
        ["WAL"] = "País de Gales",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static string For(string code) => ByCode.GetValueOrDefault(code.Trim()) ?? code.Trim().ToUpperInvariant();
}
