namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

public sealed record JolpicaDriverResponse
{
    public string DriverId { get; init; } = string.Empty;

    public string? Url { get; init; }

    public string GivenName { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    /// <summary>Adjetivo em inglês, como "Brazilian".</summary>
    public string Nationality { get; init; } = string.Empty;
}
