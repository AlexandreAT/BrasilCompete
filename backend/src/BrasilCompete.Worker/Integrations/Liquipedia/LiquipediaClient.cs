using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

using BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// API do MediaWiki da Liquipedia, com <c>action=query</c> (wikitext) e <c>action=expandtemplates</c> (nomes dos times).
/// Termos: no máximo uma requisição a cada 2 segundos (30 segundos para as requisições pesadas, como <c>parse</c>),
/// respostas em gzip, User-Agent com contato e atribuição (CC BY-SA 3.0). Páginas HTML não são lidas.
/// </summary>
public sealed class LiquipediaClient(IHttpClientFactory httpClientFactory, TimeProvider timeProvider)
{
    public const string ClientName = LiquipediaEventSource.SourceName;

    public const string MatchNamespace = "130";

    private const int TitlesPerRequest = 50;

    /// <summary>
    /// Os termos só citam o <c>parse</c> como pesado; por cautela, o <c>expandtemplates</c> segue o mesmo intervalo.
    /// </summary>
    private static readonly TimeSpan HeavyRequestInterval = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly SemaphoreSlim heavyRequestGate = new(1, 1);

    private DateTimeOffset? lastHeavyRequestAt;

    /// <summary>
    /// Wikitext de várias páginas, em lotes de até 50 títulos por requisição, indexado pelo título pedido.
    /// Segue a normalização de títulos e os redirecionamentos ("furia" → "Furia" → "FURIA").
    /// </summary>
    public async Task<IReadOnlyDictionary<string, LiquipediaPage>> GetPagesAsync(
        string wiki,
        IEnumerable<string> titles,
        CancellationToken cancellationToken)
    {
        var pages = new Dictionary<string, LiquipediaPage>(StringComparer.OrdinalIgnoreCase);

        foreach (var batch in titles.Distinct(StringComparer.OrdinalIgnoreCase).Chunk(TitlesPerRequest))
        {
            var query = $"{wiki}/api.php?action=query&prop=revisions&rvprop=content&rvslots=main&redirects=1&format=json&formatversion=2&titles={Uri.EscapeDataString(string.Join('|', batch))}";
            var result = (await GetAsync(query, cancellationToken)).Query;

            if (result is null)
            {
                continue;
            }

            var byTitle = result.Pages
                .Where(page => page.Content is not null)
                .ToDictionary(page => page.Title, page => new LiquipediaPage(page.Title, page.Content!), StringComparer.OrdinalIgnoreCase);
            var normalized = result.Normalized.ToDictionary(item => item.From, item => item.To, StringComparer.OrdinalIgnoreCase);
            var redirects = result.Redirects.ToDictionary(item => item.From, item => item.To, StringComparer.OrdinalIgnoreCase);

            foreach (var requested in batch)
            {
                var title = normalized.GetValueOrDefault(requested, requested);
                title = redirects.GetValueOrDefault(title, title);

                if (byTitle.TryGetValue(title, out var page))
                {
                    pages[requested] = page;
                }
            }
        }

        return pages;
    }

    /// <summary>Títulos das páginas de um namespace com um prefixo (ex.: as partidas <c>Match:ID CHAMP26</c>).</summary>
    public async Task<IReadOnlyList<string>> ListPagesAsync(
        string wiki,
        string ns,
        string prefix,
        CancellationToken cancellationToken)
    {
        var titles = new List<string>();
        string? next = null;

        do
        {
            var query = $"{wiki}/api.php?action=query&list=allpages&apnamespace={ns}&apprefix={Uri.EscapeDataString(prefix)}&aplimit=500&format=json&formatversion=2"
                + (next is null ? string.Empty : $"&apcontinue={Uri.EscapeDataString(next)}");
            var response = await GetAsync(query, cancellationToken);
            titles.AddRange(response.Query?.Allpages.Select(page => page.Title) ?? []);
            next = response.Continue?.Apcontinue;
        }
        while (next is not null);

        return titles;
    }

    /// <summary>
    /// Página de cada time a partir do nome usado nas chaves ("furia" → "FURIA", "vit" → "Team Vitality"),
    /// pela predefinição <c>{{TeamPage|...}}</c>, numa requisição só. Nomes sem predefinição ficam de fora.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> GetTeamPageTitlesAsync(
        string wiki,
        IReadOnlyList<string> teamNames,
        CancellationToken cancellationToken)
    {
        if (teamNames.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var text = LiquipediaWikitextReader.BuildTeamPageText(teamNames);
        var query = $"{wiki}/api.php?action=expandtemplates&prop=wikitext&format=json&formatversion=2&text={Uri.EscapeDataString(text)}";

        await heavyRequestGate.WaitAsync(cancellationToken);

        try
        {
            if (lastHeavyRequestAt is { } last && timeProvider.GetUtcNow() - last < HeavyRequestInterval)
            {
                await Task.Delay(HeavyRequestInterval - (timeProvider.GetUtcNow() - last), timeProvider, cancellationToken);
            }

            var response = await httpClientFactory.CreateClient(ClientName).GetFromJsonAsync<LiquipediaExpandResponse>(query, SerializerOptions, cancellationToken);
            lastHeavyRequestAt = timeProvider.GetUtcNow();

            return LiquipediaWikitextReader.ParseTeamPages(teamNames, response?.Expandtemplates?.Wikitext ?? string.Empty);
        }
        finally
        {
            heavyRequestGate.Release();
        }
    }

    public static string BuildPageUrl(string wiki, string title) =>
        string.Create(CultureInfo.InvariantCulture, $"https://liquipedia.net/{wiki}/{Uri.EscapeDataString(title.Replace(' ', '_')).Replace("%2F", "/", StringComparison.Ordinal)}");

    private async Task<LiquipediaQueryResponse> GetAsync(string path, CancellationToken cancellationToken) =>
        await httpClientFactory.CreateClient(ClientName).GetFromJsonAsync<LiquipediaQueryResponse>(path, SerializerOptions, cancellationToken)
        ?? new LiquipediaQueryResponse();
}
