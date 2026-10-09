using System.Globalization;
using System.Text.RegularExpressions;

using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;
using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>
/// Lê as chaves de tênis (predefinições <c>...Bracket-Tennis...</c>) pelos parâmetros <c>RD{rodada}-team{posição}</c>:
/// em cada rodada, as posições ímpar e par seguinte formam um confronto.
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
            var slots = bracket.Parameters
                .Select(parameter => (Match: TeamParameterPattern().Match(parameter.Key), parameter.Value))
                .Where(item => item.Match.Success)
                .Select(item => (
                    Round: int.Parse(item.Match.Groups["round"].Value, CultureInfo.InvariantCulture),
                    Slot: int.Parse(item.Match.Groups["slot"].Value, CultureInfo.InvariantCulture),
                    Player: ParsePlayer(item.Value)))
                .ToList();

            foreach (var round in slots.GroupBy(slot => slot.Round))
            {
                var ordered = round.OrderBy(slot => slot.Slot).ToList();

                for (var index = 0; index + 1 < ordered.Count; index += 2)
                {
                    matches.Add(new TennisDrawMatch(bracket.Heading, round.Key, ordered[index].Slot, ordered[index].Player, ordered[index + 1].Player));
                }
            }
        }

        return matches;
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
