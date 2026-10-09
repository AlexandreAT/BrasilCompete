namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>Um confronto da chave. Um lado <c>null</c> ainda não está definido (ou é um bye).</summary>
public sealed record TennisDrawMatch(string? Section, int Round, int Slot, TennisPlayer? First, TennisPlayer? Second);
