namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaCircuitResponse
{
    public string CircuitId { get; init; } = string.Empty;

    public string CircuitName { get; init; } = string.Empty;

    public JolpicaLocationResponse? Location { get; init; }
}
