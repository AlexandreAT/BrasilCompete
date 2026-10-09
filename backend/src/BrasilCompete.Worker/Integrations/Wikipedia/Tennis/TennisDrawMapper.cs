using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>
/// Os confrontos da chave viram eventos com "Período" (as datas do torneio): a Wikipedia não traz a ordem de jogos,
/// então o dia de cada partida fica desconhecido. Com um lado indefinido, vira a participação do lado conhecido.
/// </summary>
public static class TennisDrawMapper
{
    public static SportEvent? ToEvent(TennisDrawMatch match, WikipediaPageOptions draw, DateTimeOffset retrievedAtUtc)
    {
        var players = new[] { match.First, match.Second }.OfType<TennisPlayer>().ToList();

        if (players.Count == 0 || draw.PeriodStart is not { } start || draw.PeriodEnd is not { } end)
        {
            return null;
        }

        return new SportEvent
        {
            Sport = Sport.Tennis,
            Competition = draw.Competition ?? draw.Title.Replace('_', ' '),
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
                    ExternalId = $"{draw.Title}#{match.Section}-RD{match.Round}-{match.Slot}",
                },
            ],
            Confidence = Confidence.Medium,
        };
    }

    private static string StageOf(TennisDrawMatch match) =>
        string.Equals(match.Section, "Finals", StringComparison.OrdinalIgnoreCase)
            ? match.Round switch
            {
                1 => "Quartas de final",
                2 => "Semifinal",
                _ => "Final",
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
