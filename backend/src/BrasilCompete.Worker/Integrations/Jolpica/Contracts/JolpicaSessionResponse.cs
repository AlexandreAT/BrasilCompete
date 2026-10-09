namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaSessionResponse
{
    public string Date { get; init; } = string.Empty;

    public string? Time { get; init; }
}
