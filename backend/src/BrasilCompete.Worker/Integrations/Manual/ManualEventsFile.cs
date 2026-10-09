namespace BrasilCompete.Worker.Integrations.Manual;

/// <summary>Formato do arquivo versionado de curadoria (<c>backend/curation/manual-events.json</c>).</summary>
public sealed record ManualEventsFile
{
    public IReadOnlyList<ManualEventEntry> Events { get; init; } = [];
}
