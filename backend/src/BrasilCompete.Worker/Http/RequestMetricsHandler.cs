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
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            // Falha de rede ou tempo esgotado: conta como falha (o cancelamento pelo usuário também passa por aqui).
            metrics.Record(source, null);
            throw;
        }
    }
}
