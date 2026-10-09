namespace BrasilCompete.Worker.History;

public sealed record EventHistoryFile
{
    public Dictionary<string, EventHistoryEntry> Events { get; init; } = [];
}
