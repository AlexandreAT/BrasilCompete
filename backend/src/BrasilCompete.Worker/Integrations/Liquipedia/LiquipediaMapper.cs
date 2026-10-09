using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// Uma partida vira um confronto entre organizações, com o país vindo do infobox de cada time.
/// Com um oponente ainda indefinido, vira a participação do time conhecido naquela fase.
/// </summary>
public static class LiquipediaMapper
{
    public const string License = "CC BY-SA 3.0 (Liquipedia; somente fatos)";

    public const string TeamIdKey = "liquipedia";

    public static SportEvent? ToEvent(
        LiquipediaMatch match,
        LiquipediaTournamentOptions tournament,
        IReadOnlyDictionary<string, string> stages,
        IReadOnlyDictionary<string, LiquipediaTeam> teamsByKey,
        IReadOnlyDictionary<string, TimeSpan> timezones,
        DateTimeOffset retrievedAtUtc)
    {
        var teams = new[] { match.FirstTeam, match.SecondTeam }.OfType<string>().ToList();

        if (teams.Count == 0)
        {
            return null;
        }

        return new SportEvent
        {
            Sport = tournament.Sport,
            Competition = tournament.Competition,
            Stage = stages.GetValueOrDefault(match.BracketId),
            Format = teams.Count == 2 ? EventFormat.Matchup : EventFormat.Participation,
            Scope = tournament.Scope,
            Schedule = ToSchedule(match, timezones),
            Participants = teams.Select(key => ToParticipant(key, tournament.Wiki, teamsByKey)).ToList(),
            Sources =
            [
                new SourceReference
                {
                    Source = LiquipediaEventSource.SourceName,
                    Url = LiquipediaClient.BuildPageUrl(tournament.Wiki, tournament.Page),
                    License = License,
                    RetrievedAtUtc = retrievedAtUtc,
                    ExternalId = $"{tournament.Wiki}/{match.PageTitle}",
                },
            ],
        };
    }

    private static Participant ToParticipant(string key, string wiki, IReadOnlyDictionary<string, LiquipediaTeam> teamsByKey)
    {
        var team = teamsByKey.GetValueOrDefault(key) ?? new LiquipediaTeam(key, null);

        return new Participant
        {
            Name = team.Name,
            Kind = ParticipantKind.Organization,
            Country = CountryCodes.FromEnglishName(team.Location),
            ExternalIds = new Dictionary<string, string> { [TeamIdKey] = $"{wiki}/{team.Name}" },
        };
    }

    private static Schedule ToSchedule(LiquipediaMatch match, IReadOnlyDictionary<string, TimeSpan> timezones)
    {
        if (match.Date is not { } date)
        {
            return Schedule.ToBeConfirmed();
        }

        if (match.LocalTime is { } time
            && match.TimezoneAbbreviation is { } abbreviation
            && timezones.TryGetValue(abbreviation, out var offset))
        {
            return Schedule.AtTime(new DateTimeOffset(date.ToDateTime(time), offset), abbreviation);
        }

        return Schedule.OnDate(date);
    }
}
