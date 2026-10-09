using System.Globalization;
using System.Text.RegularExpressions;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// Offsets das siglas de fuso usadas nas partidas (<c>{{Abbr/CEST}}</c>), lidos do módulo de dados
/// <c>Module:Timezone/Data</c> da própria Liquipedia. Assim "CST" é o que a Liquipedia define, sem adivinhação.
/// </summary>
public static partial class LiquipediaTimezones
{
    public static IReadOnlyDictionary<string, TimeSpan> Parse(string moduleContent)
    {
        var offsets = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in EntryPattern().Matches(moduleContent))
        {
            var hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
            var minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
            var offset = new TimeSpan(Math.Abs(hours), Math.Abs(minutes), 0);

            offsets.TryAdd(match.Groups["key"].Value, hours < 0 || minutes < 0 ? -offset : offset);
        }

        return offsets;
    }

    [GeneratedRegex(@"^\s*(?<key>[A-Za-z][A-Za-z0-9]*)\s*=\s*\{[^{}]*?offset\s*=\s*\{\s*(?<hours>-?\d+)\s*,\s*(?<minutes>-?\d+)\s*\}", RegexOptions.Multiline)]
    private static partial Regex EntryPattern();
}
