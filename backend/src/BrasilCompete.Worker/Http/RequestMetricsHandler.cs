namespace BrasilCompete.Worker.Http;

public sealed class RequestMetricsHandler(string source, RequestMetrics metrics) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            metrics.Record(source, (int)response.StatusCode);

            return response;
        }
        catch (HttpRequestException)
        {
            metrics.Record(source, null);
            throw;
        }
    }
}
