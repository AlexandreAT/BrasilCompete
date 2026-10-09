using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>
/// Os confrontos da chave viram eventos com "Período" (as datas do torneio): a Wikipedia não traz a ordem de jogos,
/// então o dia de cada partida fica desconhecido. Com um lado indefinido, vira a participação do lado conhecido.
/// Vale para o tênis e o tênis de mesa (a modalidade vem da configuração da página).
/// </summary>
public static class TennisDrawMapper
{
    private static readonly Dictionary<string, string> EventNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Men's singles"] = "simples masculino",
        ["Women's singles"] = "simples feminino",
    };

    public static SportEvent? ToEvent(TennisDrawMatch match, WikipediaPageOptions draw, DateTimeOffset retrievedAtUtc)
    {
        var players = new[] { match.First, match.Second }.OfType<TennisPlayer>().ToList();

        if (players.Count == 0 || match.IsDoubles || draw.PeriodStart is not { } start || draw.PeriodEnd is not { } end)
        {
            return null;
        }

        return new SportEvent
        {
            Sport = draw.Sport,
            Competition = CompetitionOf(match, draw),
            Stage = StageOf(match),
            Format = players.Count == 2 ? EventFormat.Matchup : EventFormat.Participation,
            Scope = CompetitionScope.International,
            Schedule = Schedule.InPeriod(start, end),
            Participants = players.Select(ToParticipant).ToList(),
            Sources =
            [
                new SourceReference
                {
                    Source = WikipediaClient.SourceName,
                    Url = WikipediaClient.BuildArticleUrl(draw.Title),
                    License = WikipediaClient.License,
                    RetrievedAtUtc = retrievedAtUtc,
                    ExternalId = $"{draw.Title}#{TextNormalizer.Slugify(match.Event ?? string.Empty)}-{match.Section}-RD{match.Round}-{match.Slot}",
                },
            ],
            Confidence = Confidence.Medium,
        };
    }

    /// <summary>"WTT Champions Macao 2026 — simples masculino" quando a página tem mais de uma prova.</summary>
    private static string CompetitionOf(TennisDrawMatch match, WikipediaPageOptions draw)
    {
        var competition = draw.Competition ?? draw.Title.Replace('_', ' ');

        return match.Event is { } name && EventNames.TryGetValue(name, out var eventName)
            ? $"{competition} — {eventName}"
            : competition;
    }

    /// <summary>Na seção "Finals", a fase vem de quantas rodadas faltam para o fim da chave (de 8 ou de 4).</summary>
    private static string StageOf(TennisDrawMatch match) =>
        string.Equals(match.Section, "Finals", StringComparison.OrdinalIgnoreCase)
            ? (match.RoundCount - match.Round) switch
            {
                0 => "Final",
                1 => "Semifinal",
                2 => "Quartas de final",
                _ => "Oitavas de final",
            }
            : $"{match.Section} · Rodada {match.Round}";

    private static Participant ToParticipant(TennisPlayer player) => new()
    {
        Name = player.Name,
        Kind = ParticipantKind.Athlete,
        Country = player.Country,
        ExternalIds = player.WikipediaTitle is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { [ExternalIdKeys.EnglishWikipedia] = player.WikipediaTitle },
    };
}
