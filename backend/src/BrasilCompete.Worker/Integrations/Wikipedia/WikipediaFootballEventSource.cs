using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Wikipedia.Football;
using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>
/// Futebol pela Wikipedia: seleções brasileiras e competições internacionais de clubes do catálogo,
/// lidas das predefinições de partida no HTML do Parsoid.
/// </summary>
public sealed class WikipediaFootballEventSource(
    WikipediaClient client,
    IOptions<WikipediaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "wikipedia-football";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var retrievedAt = timeProvider.GetUtcNow();
        var events = new List<SportEvent>();
        var warnings = new List<string>();
        var notes = new List<string>();

        foreach (var page in options.Value.FootballPages)
        {
            string html;

            try
            {
                html = await client.GetPageHtmlAsync(SourceName, page.Title, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                warnings.Add($"Página {page.Title} não lida: {exception.Message}");
                continue;
            }

            var boxes = ParsoidReader.ReadTemplates(html, FootballBoxMapper.IsFootballBox);
            var mapped = FootballBoxMapper.ToEvents(boxes, page, window.From.Year, retrievedAt);
            var inWindow = mapped.Where(sportEvent => window.Overlaps(sportEvent.Schedule)).ToList();
            var withoutOffset = boxes.Count(box => FootballBoxFields.HasTimeWithoutOffset(box.Parameters.GetValueOrDefault("time")));

            events.AddRange(inWindow);
            notes.Add($"{page.Title}: {boxes.Count} partidas na página, {inWindow.Count} na janela, {withoutOffset} com horário sem fuso (usadas só com a data).");
        }

        return new SourceCollection(events, warnings, notes);
    }
}
