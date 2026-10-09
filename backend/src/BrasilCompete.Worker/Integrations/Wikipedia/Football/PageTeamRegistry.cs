using System.Text.RegularExpressions;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;
using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Football;

/// <summary>
/// Times de uma página. A Wikipedia costuma pôr link e bandeira só na primeira vez que o time aparece
/// (o jogo de ida); no jogo de volta vem só o nome. O registro resolve o nome pelos jogos já identificados.
/// </summary>
public sealed partial class PageTeamRegistry
{
    private readonly Dictionary<string, Participant> byName = new(StringComparer.Ordinal);

    public PageTeamRegistry(IEnumerable<ParsoidTemplate> boxes)
    {
        foreach (var box in boxes)
        {
            foreach (var field in new[] { "team1", "team2" })
            {
                if (box.Parameters.TryGetValue(field, out var wikitext) && FootballBoxFields.ParseTeam(wikitext) is { } team)
                {
                    Register(team.Name, team);

                    if (team.ExternalIds.TryGetValue(ExternalIdKeys.EnglishWikipedia, out var title))
                    {
                        Register(title, team);
                    }
                }
            }
        }
    }

    /// <summary>
    /// O time do campo: identificado pela própria predefinição, pelo nome já visto na página, ou só pelo nome.
    /// Marcadores como "Winner of Match QF1" ou "Higher-seeded finalist" devolvem <c>null</c>.
    /// </summary>
    public Participant? Resolve(string? wikitext)
    {
        if (FootballBoxFields.ParseTeam(wikitext) is { } team)
        {
            return team;
        }

        var name = wikitext is null ? string.Empty : WikitextReader.ToPlainText(wikitext);

        if (name.Length == 0 || PlaceholderPattern().IsMatch(name))
        {
            return null;
        }

        return byName.TryGetValue(TextNormalizer.NormalizeName(name), out var known)
            ? known
            : new Participant { Name = name, Kind = ParticipantKind.Club };
    }

    private void Register(string name, Participant team) => byName.TryAdd(TextNormalizer.NormalizeName(name), team);

    [GeneratedRegex(@"\b(TBD|TBA|winner|loser|finalist|seeded|match|group|runner|qualifier)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PlaceholderPattern();
}
