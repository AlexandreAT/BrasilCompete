namespace BrasilCompete.Worker.Http;

public sealed class PoliteDelayHandler(PolitenessGate gate) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        gate.RunAsync(() => base.SendAsync(request, cancellationToken), cancellationToken);
}
