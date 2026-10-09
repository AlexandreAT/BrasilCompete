using System.Net.Http.Headers;
using System.Text.Json;

using BrasilCompete.Worker.Identity.Wikidata.Contracts;

namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>
/// Cliente do Wikidata Query Service. Limites do serviço: 60 s por consulta e 60 s de processamento por minuto
/// por User-Agent e IP; o 429 traz <c>Retry-After</c>.
/// </summary>
public sealed class WikidataClient(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<SparqlResponse> QueryAsync(string sparql, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(WikidataOptions.ClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"sparql?format=json&query={Uri.EscapeDataString(sparql)}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<SparqlResponse>(stream, SerializerOptions, cancellationToken)
            ?? new SparqlResponse();
    }
}
