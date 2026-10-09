namespace BrasilCompete.Worker.History;

/// <summary>
/// Vida de um evento entre execuções: primeira aparição, mudanças e sumiço (plano, seção 9.9).
/// </summary>
public sealed record EventHistoryEntry
{
    public required string MatchKey { get; init; }

    public required DateTimeOffset FirstSeenUtc { get; init; }

    /// <summary>Primeira aparição com pelo menos "Data marcada"; base da métrica de antecedência.</summary>
    public DateTimeOffset? FirstSeenWithDateUtc { get; init; }

    public required DateTimeOffset LastSeenUtc { get; init; }

    /// <summary>Quando o evento deixou de aparecer. Um evento ausente não é apagado (guia, seção 9).</summary>
    public DateTimeOffset? MissingSinceUtc { get; init; }

    public required EventSnapshot Snapshot { get; init; }

    public IReadOnlyList<EventChange> Changes { get; init; } = [];
}
