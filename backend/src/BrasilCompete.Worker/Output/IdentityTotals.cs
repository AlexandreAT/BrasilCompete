namespace BrasilCompete.Worker.Output;

/// <summary>
/// Quem decidiu que cada participante é brasileiro, quantos casaram só pelo nome (baixa confiança)
/// e quantos ficaram de fora por terem só a cidadania.
/// </summary>
public sealed record IdentityTotals(
    int BySource,
    int ByWikidata,
    int ByManual,
    int LowConfidence,
    int CitizenshipOnly);
