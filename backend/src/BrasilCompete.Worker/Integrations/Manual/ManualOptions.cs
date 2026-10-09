namespace BrasilCompete.Worker.Integrations.Manual;

public sealed class ManualOptions
{
    public const string SectionName = "Sources:Manual";

    public bool Enabled { get; set; } = true;

    /// <summary>Arquivo de curadoria, relativo à pasta <c>backend/</c>.</summary>
    public string FilePath { get; set; } = "curation/manual-events.json";
}
