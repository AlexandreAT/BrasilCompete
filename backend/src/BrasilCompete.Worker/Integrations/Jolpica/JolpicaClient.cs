using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

using BrasilCompete.Worker.Integrations.Jolpica.Contracts;

namespace BrasilCompete.Worker.Integrations.Jolpica;

/// <summary>
/// API da Jolpica-F1 (compatível com o Ergast). Uso não comercial, dados em CC BY-NC-SA 4.0;
/// limites de 4 requisições por segundo e 500 por hora.
/// </summary>
public sealed class JolpicaClient(IHttpClientFactory httpClientFactory)
{
    public const string ClientName = JolpicaEventSource.SourceName;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<JolpicaRaceResponse>> GetRacesAsync(int season, CancellationToken cancellationToken)
    {
        var response = await GetAsync($"{Season(season)}/races/?limit=100", cancellationToken);

        return response.Data.RaceTable?.Races ?? [];
    }

    /// <summary>Classificação mais recente, com a equipe de cada piloto.</summary>
    public async Task<IReadOnlyList<JolpicaDriverStandingResponse>> GetDriverStandingsAsync(int season, CancellationToken cancellationToken)
    {
        var response = await GetAsync($"{Season(season)}/driverstandings/?limit=100", cancellationToken);

        return response.Data.StandingsTable?.StandingsLists.LastOrDefault()?.DriverStandings ?? [];
    }

    /// <summary>Pilotos da temporada, para quando ainda não há classificação (início de temporada).</summary>
    public async Task<IReadOnlyList<JolpicaDriverResponse>> GetDriversAsync(int season, CancellationToken cancellationToken)
    {
        var response = await GetAsync($"{Season(season)}/drivers/?limit=100", cancellationToken);

        return response.Data.DriverTable?.Drivers ?? [];
    }

    public static string BuildRaceUrl(string season, string round) => $"https://api.jolpi.ca/ergast/f1/{season}/{round}/";

    private async Task<JolpicaResponse> GetAsync(string path, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(ClientName);

        return await client.GetFromJsonAsync<JolpicaResponse>(path, SerializerOptions, cancellationToken)
            ?? new JolpicaResponse();
    }

    private static string Season(int season) => season.ToString(CultureInfo.InvariantCulture);
}
