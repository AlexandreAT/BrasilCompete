namespace BrasilCompete.Worker.Domain;

/// <summary>
/// Camada que decidiu se o participante é brasileiro (plano, seção 9.5).
/// </summary>
public enum IdentityLayer
{
    Source,
    Wikidata,
    Manual,
}
