using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>Tênis pelas chaves da Wikipedia, para os torneios do catálogo que cruzam a janela.</summary>
public sealed class WikipediaTennisEventSource(
    WikipediaClient client,
    IOptions<WikipediaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "wikipedia-tennis";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var events = new List<SportEvent>();
        var warnings = new List<string>();
        var notes = new List<string>();

        foreach (var draw in options.Value.TennisDraws.Where(draw => Overlaps(draw, window)))
        {
            string html;

            try
            {
                html = await client.GetPageHtmlAsync(SourceName, draw.Title, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                warnings.Add($"Chave {draw.Title} não lida: {exception.Message}");
                continue;
            }

            var matches = TennisDrawReader.Read(html);
            var mapped = matches.Select(match => TennisDrawMapper.ToEvent(match, draw, retrievedAt)).OfType<SportEvent>().ToList();
            events.AddRange(mapped);
            notes.Add($"{draw.Title}: {matches.Count} confrontos na chave ({matches.Count(match => match.First is null || match.Second is null)} com um lado indefinido).");
        }

        return new SourceCollection(events, warnings, notes);
    }

    private static bool Overlaps(WikipediaPageOptions draw, DateWindow window) =>
        draw.PeriodStart is { } start && draw.PeriodEnd is { } end && start <= window.To && end >= window.From;
}
