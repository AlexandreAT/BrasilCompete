namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>
/// Um confronto da chave. <see cref="Event"/> é a prova (título de nível 2, como "Men's singles"), quando a página
/// tem mais de uma. Um lado <c>null</c> ainda não está definido (ou é um bye).
/// </summary>
public sealed record TennisDrawMatch(
    string? Event,
    string? Section,
    int Round,
    int RoundCount,
    int Slot,
    TennisPlayer? First,
    TennisPlayer? Second)
{
    /// <summary>Chaves de duplas trazem dois atletas por lado e ainda não são lidas.</summary>
    public bool IsDoubles => Event?.Contains("doubles", StringComparison.OrdinalIgnoreCase) == true;
}
