using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BrasilCompete.Worker.Http;

public sealed class ResponseCacheHandler(
    string source,
    TimeSpan ttl,
    HttpResponseCache cache,
    TimeProvider timeProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var key = HttpResponseCache.CreateKey(request);
        var cached = await cache.TryGetAsync(source, key, ttl, cancellationToken);

        if (cached is not null)
        {
            return ToResponse(cached, request);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return response;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var stored = new CachedResponse(
            request.RequestUri!.ToString(),
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.ToString(),
            timeProvider.GetUtcNow(),
            body);

        await cache.StoreAsync(source, key, stored, cancellationToken);
        response.Dispose();

        return ToResponse(stored, request);
    }

    private static HttpResponseMessage ToResponse(CachedResponse cached, HttpRequestMessage request)
    {
        var content = new StringContent(cached.Body, Encoding.UTF8);

        if (cached.ContentType is not null)
        {
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(cached.ContentType);
        }

        return new HttpResponseMessage((HttpStatusCode)cached.StatusCode)
        {
            Content = content,
            RequestMessage = request,
        };
    }
}
