using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations;

/// <summary>
/// Resultado de uma fonte. Avisos indicam coleta parcial (parte dos dados não pôde ser lida);
/// notas são informações para as métricas, como quantos torneios foram verificados.
/// </summary>
public sealed record SourceCollection(
    IReadOnlyList<SportEvent> Events,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string>? Notes = null)
{
    public static SourceCollection Empty { get; } = new([], []);
}
