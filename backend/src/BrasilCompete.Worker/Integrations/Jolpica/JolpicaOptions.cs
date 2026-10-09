using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Jolpica;

public sealed class JolpicaOptions
{
    public const string SectionName = "Sources:Jolpica";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Sessões que viram eventos próprios. Os treinos livres ficam de fora por padrão
    /// (plano, seção 3.1: decidir e medir o volume das duas opções).
    /// </summary>
    public List<JolpicaSession> Sessions { get; set; } =
    [
        JolpicaSession.Race,
        JolpicaSession.Qualifying,
        JolpicaSession.Sprint,
        JolpicaSession.SprintQualifying,
    ];

    public SourceHttpOptions Http { get; set; } = new();
}
