namespace BrasilCompete.Worker.Domain;

/// <summary>
/// De onde veio o evento, para atribuição e auditoria (plano, regra 6.1.7).
/// </summary>
public sealed record SourceReference
{
    public required string Source { get; init; }

    public required string Url { get; init; }

    public required string License { get; init; }

    public required DateTimeOffset RetrievedAtUtc { get; init; }

    /// <summary>Identificador do evento na própria fonte, quando existir.</summary>
    public string? ExternalId { get; init; }
}
