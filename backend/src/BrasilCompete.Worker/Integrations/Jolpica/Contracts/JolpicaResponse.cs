using System.Text.Json.Serialization;

namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

/// <summary>Envelope das respostas da API compatível com o Ergast (<c>MRData</c>).</summary>
public sealed record JolpicaResponse
{
    [JsonPropertyName("MRData")]
    public JolpicaDataResponse Data { get; init; } = new();
}
