using BrasilCompete.Worker.Http;

namespace BrasilCompete.Worker.Integrations.Jolpica;

public sealed class JolpicaOptions
{
    public const string SectionName = "Sources:Jolpica";

    private static readonly JolpicaSession[] DefaultSessions =
    [
        JolpicaSession.Race,
        JolpicaSession.Qualifying,
        JolpicaSession.Sprint,
        JolpicaSession.SprintQualifying,
    ];

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Sessões que viram eventos próprios. Os treinos livres ficam de fora por padrão
    /// (plano, seção 3.1: decidir e medir o volume das duas opções). Começa vazia porque o binder da configuração
    /// acrescenta os itens a uma lista já preenchida, em vez de substituí-la (as sessões saíam repetidas).
    /// </summary>
    public List<JolpicaSession> Sessions { get; set; } = [];

    /// <summary>As sessões configuradas, sem repetição, ou as padrão quando nenhuma foi configurada.</summary>
    public IReadOnlyList<JolpicaSession> EffectiveSessions =>
        Sessions.Count == 0 ? DefaultSessions : Sessions.Distinct().ToList();

    public SourceHttpOptions Http { get; set; } = new();
}
