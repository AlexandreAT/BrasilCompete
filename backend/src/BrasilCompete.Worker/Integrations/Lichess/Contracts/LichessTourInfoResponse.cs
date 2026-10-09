namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

public sealed record LichessTourInfoResponse
{
    public string? Location { get; init; }

    /// <summary>Fuso IANA do local do torneio.</summary>
    public string? TimeZone { get; init; }

    public string? Format { get; init; }
}
