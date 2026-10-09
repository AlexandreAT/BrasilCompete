namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaLocationResponse
{
    public string Locality { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;
}
