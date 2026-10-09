using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Wikipedia.Mma;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>UFC pela Wikipedia: a lista de eventos e o card anunciado de cada evento da janela.</summary>
public sealed class WikipediaUfcEventSource(
    WikipediaClient client,
    IOptions<WikipediaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "wikipedia-ufc";

    private const string EventListTitle = "List_of_UFC_events";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var listings = UfcPageReader.ReadEventList(await client.GetPageHtmlAsync(SourceName, EventListTitle, cancellationToken))
            .Where(listing => listing.Date >= window.From && listing.Date <= window.To)
            .DistinctBy(listing => listing.Name)
            .ToList();

        var events = new List<SportEvent>();
        var warnings = new List<string>();
        var withoutCard = 0;

        foreach (var listing in listings)
        {
            if (listing.Title is null)
            {
                withoutCard++;
                continue;
            }

            var bouts = UfcPageReader.ReadFightCard(await client.GetPageHtmlAsync(SourceName, listing.Title.Replace(' ', '_'), cancellationToken));

            if (bouts.Count == 0)
            {
                withoutCard++;
            }

            events.AddRange(bouts.Select(bout => UfcMapper.ToEvent(listing, bout, retrievedAt)));
        }

        var notes = new List<string> { $"{listings.Count} eventos na janela; {withoutCard} ainda sem card publicado." };

        return new SourceCollection(events, warnings, notes);
    }
}
