namespace BrasilCompete.Worker.Http;

public sealed record SourceRequestStats(int Requests, int RateLimited, int Failures);
