namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaDriverTableResponse
{
    public IReadOnlyList<JolpicaDriverResponse> Drivers { get; init; } = [];
}
