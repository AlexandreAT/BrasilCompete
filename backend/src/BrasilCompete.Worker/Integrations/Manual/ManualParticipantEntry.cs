using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations.Manual;

public sealed record ManualParticipantEntry
{
    public required string Name { get; init; }

    public required ParticipantKind Kind { get; init; }

    public string? Country { get; init; }

    public string? Team { get; init; }

    /// <summary>Quando informado, a curadoria decide se o participante é brasileiro.</summary>
    public bool? IsBrazilian { get; init; }

    public BrazilianReason? BrazilianReason { get; init; }

    public Dictionary<string, string>? ExternalIds { get; init; }

    public IReadOnlyList<ManualParticipantEntry>? Members { get; init; }
}
