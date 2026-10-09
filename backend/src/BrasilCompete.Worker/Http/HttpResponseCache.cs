using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using BrasilCompete.Worker.Configuration;

namespace BrasilCompete.Worker.Http;

/// <summary>
/// Cache de respostas em disco, para não repetir requisições durante o desenvolvimento (plano, seção 9.9).
/// </summary>
public sealed class HttpResponseCache(BackendPaths paths, TimeProvider timeProvider)
{
    public bool IsEnabled { get; set; } = true;

    public async Task<CachedResponse?> TryGetAsync(
        string source,
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            return null;
        }

        var file = GetFile(source, key);

        if (!file.Exists)
        {
            return null;
        }

        await using var stream = file.OpenRead();
        var cached = await JsonSerializer.DeserializeAsync<CachedResponse>(stream, cancellationToken: cancellationToken);

        return cached is not null && timeProvider.GetUtcNow() - cached.FetchedAtUtc <= ttl ? cached : null;
    }

    public async Task StoreAsync(string source, string key, CachedResponse response, CancellationToken cancellationToken)
    {
        var file = GetFile(source, key);
        file.Directory!.Create();

        await using var stream = file.Create();
        await JsonSerializer.SerializeAsync(stream, response, cancellationToken: cancellationToken);
    }

    public static string CreateKey(HttpRequestMessage request)
    {
        var accept = string.Join(',', request.Headers.Accept.Select(header => header.ToString()));
        var raw = $"{request.Method} {request.RequestUri} {accept}";

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }

    private FileInfo GetFile(string source, string key) =>
        new(Path.Combine(paths.CacheDirectory, "http", source, $"{key}.json"));
}
