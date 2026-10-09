namespace BrasilCompete.Worker.Http;

public sealed record CachedResponse(
    string Url,
    int StatusCode,
    string? ContentType,
    DateTimeOffset FetchedAtUtc,
    string Body);
