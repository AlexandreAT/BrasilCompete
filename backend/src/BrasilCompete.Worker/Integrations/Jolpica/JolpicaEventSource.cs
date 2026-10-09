using BrasilCompete.Worker.Domain;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Integrations.Jolpica;

/// <summary>Fórmula 1: calendário da temporada com os horários de cada sessão e o grid com as equipes.</summary>
public sealed class JolpicaEventSource(
    JolpicaClient client,
    IOptions<JolpicaOptions> options,
    TimeProvider timeProvider) : IEventSource
{
    public const string SourceName = "jolpica";

    public string Name => SourceName;

    public bool IsEnabled => options.Value.Enabled;

    public async Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken)
    {
        var events = new List<SportEvent>();
        var warnings = new List<string>();

        for (var season = window.From.Year; season <= window.To.Year; season++)
        {
            var races = await client.GetRacesAsync(season, cancellationToken);
            var grid = await client.GetDriverStandingsAsync(season, cancellationToken);

            if (grid.Count == 0)
            {
                warnings.Add($"Temporada {season} sem classificação de pilotos: usei a lista de pilotos, sem as equipes.");
                grid = (await client.GetDriversAsync(season, cancellationToken)).Select(JolpicaMapper.ToGridEntry).ToList();
            }

            events.AddRange(JolpicaMapper.ToEvents(races, grid, options.Value.EffectiveSessions, window, timeProvider.GetUtcNow()));
        }

        return new SourceCollection(events, warnings);
    }
}
