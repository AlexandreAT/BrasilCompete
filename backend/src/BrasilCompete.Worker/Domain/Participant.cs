namespace BrasilCompete.Worker.Domain;

/// <summary>
/// Um lado de um confronto ou quem participa de um evento. Equipes e duplas podem listar
/// os atletas em <see cref="Members"/>; é assim que um brasileiro em equipe estrangeira aparece.
/// </summary>
public sealed record Participant
{
    private static readonly IReadOnlyDictionary<string, string> NoExternalIds = new Dictionary<string, string>();

    public required string Name { get; init; }

    public required ParticipantKind Kind { get; init; }

    /// <summary>País representado (atleta) ou país-sede (equipe), em ISO 3166-1 alfa-3.</summary>
    public string? Country { get; init; }

    /// <summary>Equipe do atleta no evento, como a escuderia de um piloto.</summary>
    public string? Team { get; init; }

    public bool IsBrazilian { get; init; }

    public BrazilianReason? BrazilianReason { get; init; }

    public IdentityLayer? DecidedBy { get; init; }

    public Confidence IdentityConfidence { get; init; } = Confidence.High;

    public IReadOnlyDictionary<string, string> ExternalIds { get; init; } = NoExternalIds;

    public IReadOnlyList<Participant> Members { get; init; } = [];
}
