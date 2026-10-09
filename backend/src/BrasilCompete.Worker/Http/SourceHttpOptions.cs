namespace BrasilCompete.Worker.Http;

/// <summary>
/// Regras de acesso de uma fonte: endereço, espaçamento entre requisições e cache.
/// </summary>
public sealed class SourceHttpOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Intervalo mínimo entre duas requisições. As requisições de uma fonte são sempre em série.</summary>
    public int MinIntervalMilliseconds { get; set; } = 1000;

    public int CacheTtlHours { get; set; } = 12;

    /// <summary>Espera após um 429 sem cabeçalho <c>Retry-After</c>.</summary>
    public int RetryAfter429Seconds { get; set; } = 60;
}
