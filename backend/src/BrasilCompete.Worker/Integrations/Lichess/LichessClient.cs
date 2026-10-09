using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using BrasilCompete.Worker.Integrations.Lichess.Contracts;

namespace BrasilCompete.Worker.Integrations.Lichess;

/// <summary>
/// API de transmissões da Lichess, sem autenticação. Regra da Lichess: uma requisição por vez e,
/// num 429, esperar um minuto antes de tentar de novo.
/// </summary>
public sealed class LichessClient(IHttpClientFactory httpClientFactory)
{
    public const string ClientName = LichessEventSource.SourceName;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Transmissões oficiais: as ativas primeiro, depois as terminadas mais recentes (ndjson).</summary>
    public async Task<IReadOnlyList<LichessBroadcastResponse>> GetOfficialBroadcastsAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/broadcast?nb=100");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));

        using var response = await CreateClient().SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return ParseNdjson(body);
    }

    public async Task<LichessTopResponse> GetTopPageAsync(int page, CancellationToken cancellationToken) =>
        await CreateClient().GetFromJsonAsync<LichessTopResponse>(
            $"api/broadcast/top?page={page.ToString(CultureInfo.InvariantCulture)}",
            SerializerOptions,
            cancellationToken)
        ?? new LichessTopResponse();

    public async Task<LichessTourDetailsResponse> GetTourAsync(string tourId, CancellationToken cancellationToken) =>
        await CreateClient().GetFromJsonAsync<LichessTourDetailsResponse>($"api/broadcast/{tourId}", SerializerOptions, cancellationToken)
        ?? new LichessTourDetailsResponse();

    /// <summary>Jogadores do torneio. Vem vazio enquanto nenhuma partida foi publicada.</summary>
    public async Task<IReadOnlyList<LichessPlayerResponse>> GetPlayersAsync(string tourId, CancellationToken cancellationToken) =>
        await CreateClient().GetFromJsonAsync<List<LichessPlayerResponse>>($"broadcast/{tourId}/players", SerializerOptions, cancellationToken)
        ?? [];

    public async Task<LichessRoundResponse> GetRoundAsync(string roundId, CancellationToken cancellationToken) =>
        await CreateClient().GetFromJsonAsync<LichessRoundResponse>($"api/broadcast/-/-/{roundId}", SerializerOptions, cancellationToken)
        ?? new LichessRoundResponse();

    public static IReadOnlyList<LichessBroadcastResponse> ParseNdjson(string body) =>
        body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => JsonSerializer.Deserialize<LichessBroadcastResponse>(line, SerializerOptions))
            .OfType<LichessBroadcastResponse>()
            .ToList();

    private HttpClient CreateClient() => httpClientFactory.CreateClient(ClientName);
}
