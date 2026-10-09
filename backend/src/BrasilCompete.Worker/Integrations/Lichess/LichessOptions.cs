using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Lichess;

public sealed class LichessOptions
{
    public const string SectionName = "Sources:Lichess";

    public bool Enabled { get; set; } = true;

    /// <summary>Quantas páginas de transmissões passadas ler (24 por página, até 20).</summary>
    public int MaxPastPages { get; set; } = 12;

    public SourceHttpOptions Http { get; set; } = new();
}
