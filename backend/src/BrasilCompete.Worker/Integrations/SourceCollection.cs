using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations;

/// <summary>
/// Resultado de uma fonte. Avisos indicam coleta parcial (parte dos dados não pôde ser lida).
/// </summary>
public sealed record SourceCollection(IReadOnlyList<SportEvent> Events, IReadOnlyList<string> Warnings)
{
    public static SourceCollection Empty { get; } = new([], []);
}
