using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Output;

/// <summary>
/// Totais da execução. <see cref="MergedAcrossSources"/> conta as junções entre fontes diferentes;
/// <see cref="Merged"/> inclui também repetições dentro da mesma fonte.
/// </summary>
public sealed record RunTotals(
    int Collected,
    int Accepted,
    int Main,
    int Individuals,
    IReadOnlyDictionary<DiscardReason, int> Discarded,
    int Merged,
    int MergedAcrossSources,
    int Conflicts,
    IdentityTotals Identity);
