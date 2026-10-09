using System.Globalization;
using System.Text.RegularExpressions;

using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;
using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>
/// Lê as chaves de tênis e de tênis de mesa (predefinições <c>...Bracket...</c>) pelos parâmetros
/// <c>RD{rodada}-team{posição}</c>: em cada rodada, as posições ímpar e par seguinte formam um confronto.
/// </summary>
public static partial class TennisDrawReader
{
    private static readonly HashSet<string> FlagTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        "flagicon", "flagIOCathlete", "flagathlete", "flagIOC", "flag", "flagcountry",
    };

    public static IReadOnlyList<TennisDrawMatch> Read(string html)
    {
        var matches = new List<TennisDrawMatch>();

        foreach (var bracket in ParsoidReader.ReadTemplates(html, name => name.Contains("Bracket", StringComparison.OrdinalIgnoreCase)))
        {
            var slots = NamedParameters(bracket)
                .Select(parameter => (Match: TeamParameterPattern().Match(parameter.Key), parameter.Value))
                .Where(item => item.Match.Success)
                .Select(item => (
                    Round: int.Parse(item.Match.Groups["round"].Value, CultureInfo.InvariantCulture),
                    Slot: int.Parse(item.Match.Groups["slot"].Value, CultureInfo.InvariantCulture),
                    Player: ParsePlayer(item.Value)))
                .ToList();
            var roundCount = slots.Count == 0 ? 0 : slots.Max(slot => slot.Round);

            foreach (var round in slots.GroupBy(slot => slot.Round))
            {
                var ordered = round.OrderBy(slot => slot.Slot).ToList();

                for (var index = 0; index + 1 < ordered.Count; index += 2)
                {
                    matches.Add(new TennisDrawMatch(bracket.TopHeading, bracket.Heading, round.Key, roundCount, ordered[index].Slot, ordered[index].Player, ordered[index + 1].Player));
                }
            }
        }

        return RemoveRepeatedMatches(matches);
    }

    /// <summary>
    /// Em algumas chaves a última rodada das seções se repete no começo da seção "Finals" (ex.: as quartas de final).
    /// Como numa chave eliminatória o mesmo par só se enfrenta uma vez por prova, fica o confronto da seção "Finals".
    /// </summary>
    public static IReadOnlyList<TennisDrawMatch> RemoveRepeatedMatches(IReadOnlyList<TennisDrawMatch> matches)
    {
        var finals = matches.Where(IsFinals).Select(PairKey).OfType<string>().ToHashSet(StringComparer.Ordinal);

        return matches.Where(match => IsFinals(match) || PairKey(match) is not { } key || !finals.Contains(key)).ToList();
    }

    private static bool IsFinals(TennisDrawMatch match) =>
        string.Equals(match.Section, "Finals", StringComparison.OrdinalIgnoreCase);

    private static string? PairKey(TennisDrawMatch match) =>
        match is { First: { } first, Second: { } second }
            ? $"{match.Event}|{string.Join('|', new[] { first.Name, second.Name }.Order(StringComparer.Ordinal))}"
            : null;

    /// <summary>
    /// No módulo <c>{{#invoke:Bracket|...}}</c>, o Parsoid entrega cada parâmetro como posicional, com o texto
    /// "RD1-team1 = ..."; aqui eles voltam a ser nomeados.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, string>> NamedParameters(ParsoidTemplate bracket)
    {
        foreach (var (key, value) in bracket.Parameters)
        {
            var separator = value.IndexOf('=', StringComparison.Ordinal);

            yield return int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out _) && separator > 0
                ? new(value[..separator].Trim(), value[(separator + 1)..].Trim())
                : new(key, value);
        }
    }

    /// <summary>
    /// "{{flagicon|BRA}} [[João Fonseca (tennis)|J Fonseca]]" ou "{{flagIOCathlete|[[...]]|BRA}}".
    /// Bye, "Qualifier" e posições vazias devolvem <c>null</c>.
    /// </summary>
    public static TennisPlayer? ParsePlayer(string wikitext)
    {
        if (WikitextReader.ReadFirstLink(wikitext) is not { } link)
        {
            return null;
        }

        var country = WikitextReader.ReadTemplates(wikitext)
            .Where(template => FlagTemplates.Contains(template.Name.Trim()))
            .SelectMany(template => template.Arguments)
            .Select(argument => argument.Trim())
            .FirstOrDefault(argument => CountryCodePattern().IsMatch(argument));

        return new TennisPlayer(DisambiguationPattern().Replace(link.Target, string.Empty).Trim(), link.Target.Replace('_', ' '), country);
    }

    [GeneratedRegex(@"^RD(?<round>\d+)-team(?<slot>\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex TeamParameterPattern();

    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CountryCodePattern();

    [GeneratedRegex(@"\s*\([^)]*\)\s*$")]
    private static partial Regex DisambiguationPattern();
}
