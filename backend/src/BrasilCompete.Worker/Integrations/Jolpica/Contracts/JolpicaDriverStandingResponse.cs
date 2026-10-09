namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaDriverStandingResponse
{
    public string Position { get; init; } = string.Empty;

    public JolpicaDriverResponse Driver { get; init; } = new();

    public IReadOnlyList<JolpicaConstructorResponse> Constructors { get; init; } = [];
}
