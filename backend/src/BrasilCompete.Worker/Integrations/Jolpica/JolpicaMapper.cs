using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Jolpica.Contracts;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Jolpica;

/// <summary>
/// Cada sessão escolhida vira um evento de participação por piloto, com horário marcado:
/// "Gabriel Bortoleto (Audi) — Brazilian Grand Prix — Corrida". A identidade é decidida no pipeline.
/// </summary>
public static class JolpicaMapper
{
    public const string License = "CC BY-NC-SA 4.0 (Jolpica-F1, uso não comercial)";

    public static IReadOnlyList<SportEvent> ToEvents(
        IReadOnlyList<JolpicaRaceResponse> races,
        IReadOnlyList<JolpicaDriverStandingResponse> grid,
        IReadOnlyCollection<JolpicaSession> sessions,
        DateWindow window,
        DateTimeOffset retrievedAtUtc)
    {
        var drivers = grid.Select(ToParticipant).ToList();
        var events = new List<SportEvent>();

        foreach (var race in races)
        {
            foreach (var session in sessions)
            {
                if (GetSession(race, session) is not { } timing || ToSchedule(timing) is not { } schedule || !window.Overlaps(schedule))
                {
                    continue;
                }

                events.AddRange(drivers.Select(driver => new SportEvent
                {
                    Sport = Sport.Formula1,
                    Competition = race.RaceName,
                    Stage = Label(session),
                    Format = EventFormat.Participation,
                    Scope = CompetitionScope.International,
                    Schedule = schedule,
                    Venue = Venue(race.Circuit),
                    Participants = [driver.Participant],
                    Sources =
                    [
                        new SourceReference
                        {
                            Source = JolpicaEventSource.SourceName,
                            Url = JolpicaClient.BuildRaceUrl(race.Season, race.Round),
                            License = License,
                            RetrievedAtUtc = retrievedAtUtc,
                            ExternalId = $"{race.Season}-{race.Round}-{session}-{driver.DriverId}",
                        },
                    ],
                }));
            }
        }

        return events;
    }

    public static JolpicaDriverStandingResponse ToGridEntry(JolpicaDriverResponse driver) => new() { Driver = driver };

    public static string Label(JolpicaSession session) => session switch
    {
        JolpicaSession.Race => "Corrida",
        JolpicaSession.Qualifying => "Classificação",
        JolpicaSession.Sprint => "Sprint",
        JolpicaSession.SprintQualifying => "Classificação da Sprint",
        JolpicaSession.FirstPractice => "Treino livre 1",
        JolpicaSession.SecondPractice => "Treino livre 2",
        JolpicaSession.ThirdPractice => "Treino livre 3",
        _ => session.ToString(),
    };

    private static (string DriverId, Participant Participant) ToParticipant(JolpicaDriverStandingResponse entry)
    {
        var driver = entry.Driver;
        var externalIds = new Dictionary<string, string> { [JolpicaEventSource.SourceName] = driver.DriverId };

        if (WikipediaUrls.TryGetEnglishTitle(driver.Url) is { } title)
        {
            externalIds[ExternalIdKeys.EnglishWikipedia] = title;
        }

        var participant = new Participant
        {
            Name = $"{driver.GivenName} {driver.FamilyName}".Trim(),
            Kind = ParticipantKind.Athlete,
            Country = CountryCodes.FromDemonym(driver.Nationality),
            Team = entry.Constructors.FirstOrDefault()?.Name,
            ExternalIds = externalIds,
        };

        return (driver.DriverId, participant);
    }

    private static JolpicaSessionResponse? GetSession(JolpicaRaceResponse race, JolpicaSession session) => session switch
    {
        JolpicaSession.Race => new JolpicaSessionResponse { Date = race.Date, Time = race.Time },
        JolpicaSession.Qualifying => race.Qualifying,
        JolpicaSession.Sprint => race.Sprint,
        JolpicaSession.SprintQualifying => race.SprintQualifying,
        JolpicaSession.FirstPractice => race.FirstPractice,
        JolpicaSession.SecondPractice => race.SecondPractice,
        JolpicaSession.ThirdPractice => race.ThirdPractice,
        _ => null,
    };

    /// <summary>A Jolpica informa os horários em UTC. Sem horário, o evento fica com "Data marcada".</summary>
    private static Schedule? ToSchedule(JolpicaSessionResponse session)
    {
        if (!DateOnly.TryParseExact(session.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(session.Time)
            || !TimeOnly.TryParse(session.Time.TrimEnd('Z'), CultureInfo.InvariantCulture, out var time))
        {
            return Schedule.OnDate(date);
        }

        return Schedule.AtTime(new DateTimeOffset(date.ToDateTime(time), TimeSpan.Zero), "Etc/UTC");
    }

    private static string? Venue(JolpicaCircuitResponse? circuit) =>
        circuit is null ? null : $"{circuit.CircuitName}, {circuit.Location?.Locality}".TrimEnd(',', ' ');
}
