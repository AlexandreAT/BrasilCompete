namespace BrasilCompete.Worker.Integrations.Wikipedia;

/// <summary>
/// HTML do Parsoid pela API REST do MediaWiki (<c>/w/rest.php/v1/page/{título}/html</c>).
/// Texto em CC BY-SA 4.0; só os fatos (datas, horários, participantes) são aproveitados.
/// Cada adapter usa o próprio cliente nomeado, para as métricas de requisições ficarem por fonte.
/// </summary>
public sealed class WikipediaClient(IHttpClientFactory httpClientFactory)
{
    /// <summary>Nome da fonte nas referências dos eventos (atribuição e prioridade).</summary>
    public const string SourceName = "wikipedia";

    public const string License = "CC BY-SA 4.0 (Wikipedia; somente fatos)";

    public async Task<string> GetPageHtmlAsync(string clientName, string title, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(clientName);

        return await client.GetStringAsync($"w/rest.php/v1/page/{Uri.EscapeDataString(title)}/html", cancellationToken);
    }

    public static string BuildArticleUrl(string title) => $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(title)}";
}
