using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Wikipedia.Mma;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>
/// UFC pela Wikipedia: a lista de eventos, o card anunciado de cada evento da janela e o elenco atual
/// (país de cada lutador pela bandeira).
/// </summary>
public sealed class WikipediaUfcEventSource(
    WikipediaClient client,
    IOptions<WikipediaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "wikipedia-ufc";

    private const string EventListTitle = "List_of_UFC_events";

    private const string RosterTitle = "List_of_current_UFC_fighters";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var listings = UfcPageReader.ReadEventList(await client.GetPageHtmlAsync(SourceName, EventListTitle, cancellationToken))
            .Where(listing => listing.Date >= window.From && listing.Date <= window.To)
            .DistinctBy(listing => listing.Name)
            .ToList();

        var roster = listings.Count == 0
            ? UfcRoster.Empty
            : UfcPageReader.ReadRoster(await client.GetPageHtmlAsync(SourceName, RosterTitle, cancellationToken));

        var events = new List<SportEvent>();
        var warnings = new List<string>();
        var withoutCard = 0;
        var fighters = 0;
        var withoutCountry = 0;

        foreach (var listing in listings)
        {
            if (listing.Title is null)
            {
                withoutCard++;
                continue;
            }

            var bouts = UfcPageReader.ReadFightCard(await client.GetPageHtmlAsync(SourceName, listing.Title.Replace(' ', '_'), cancellationToken))
                .Select(bout => bout with { First = WithCountry(bout.First, roster), Second = WithCountry(bout.Second, roster) })
                .ToList();

            if (bouts.Count == 0)
            {
                withoutCard++;
            }

            fighters += bouts.Count * 2;
            withoutCountry += bouts.Sum(bout => (bout.First.Country is null ? 1 : 0) + (bout.Second.Country is null ? 1 : 0));
            events.AddRange(bouts.Select(bout => UfcMapper.ToEvent(listing, bout, retrievedAt)));
        }

        var notes = new List<string>
        {
            $"{listings.Count} eventos na janela; {withoutCard} ainda sem card publicado.",
            $"{fighters} lutadores nos cards; {withoutCountry} fora do elenco atual (sem bandeira; ficam com o Wikidata). Elenco com {roster.Count} lutadores.",
        };

        return new SourceCollection(events, warnings, notes);
    }

    private static UfcFighter WithCountry(UfcFighter fighter, UfcRoster roster) =>
        roster.CountryOf(fighter) is { } country ? fighter with { Country = country } : fighter;
}
