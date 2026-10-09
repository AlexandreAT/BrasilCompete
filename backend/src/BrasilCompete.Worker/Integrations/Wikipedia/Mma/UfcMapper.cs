using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>
/// Cada luta do card vira um confronto com "Data marcada": a página não informa o horário de cada luta,
/// e a data é a do local do evento (no horário de Brasília, pode ser o dia seguinte).
/// </summary>
public static class UfcMapper
{
    public static SportEvent ToEvent(UfcEventListing listing, UfcBout bout, DateTimeOffset retrievedAtUtc) => new()
    {
        Sport = Sport.Mma,
        Competition = listing.Name,
        Stage = bout.WeightClass,
        Format = EventFormat.Matchup,
        Scope = CompetitionScope.International,
        Schedule = Schedule.OnDate(listing.Date),
        Venue = string.Join(", ", new[] { listing.Venue, listing.Location }.Where(part => !string.IsNullOrWhiteSpace(part) && part != "TBA")),
        Participants = [ToParticipant(bout.First), ToParticipant(bout.Second)],
        Sources =
        [
            new SourceReference
            {
                Source = WikipediaClient.SourceName,
                Url = WikipediaClient.BuildArticleUrl((listing.Title ?? listing.Name).Replace(' ', '_')),
                License = WikipediaClient.License,
                RetrievedAtUtc = retrievedAtUtc,
                ExternalId = $"{listing.Title ?? listing.Name}#{bout.First.Name}-x-{bout.Second.Name}",
            },
        ],
        Confidence = Confidence.Medium,
    };

    private static Participant ToParticipant(UfcFighter fighter) => new()
    {
        Name = fighter.Name,
        Kind = ParticipantKind.Athlete,
        ExternalIds = fighter.WikipediaTitle is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { [ExternalIdKeys.EnglishWikipedia] = fighter.WikipediaTitle },
    };
}
