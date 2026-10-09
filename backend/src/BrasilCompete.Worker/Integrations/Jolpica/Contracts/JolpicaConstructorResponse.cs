namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaConstructorResponse
{
    public string ConstructorId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
