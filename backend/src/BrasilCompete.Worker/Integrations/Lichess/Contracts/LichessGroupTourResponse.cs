namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessGroupTourResponse
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
