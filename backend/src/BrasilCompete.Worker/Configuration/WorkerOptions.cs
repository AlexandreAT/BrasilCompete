namespace BrasilCompete.Worker.Configuration;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>Identificação das requisições, com nome e contato (plano, regra 6.1.4).</summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>Ordem de prioridade das fontes na deduplicação. A curadoria manual vence todas.</summary>
    public List<string> SourcePriority { get; set; } = [];
}
