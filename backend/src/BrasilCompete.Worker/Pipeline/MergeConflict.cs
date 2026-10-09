namespace BrasilCompete.Worker.Pipeline;

/// <summary>Fontes que discordam sobre o mesmo evento (por exemplo, horários diferentes).</summary>
public sealed record MergeConflict(string EventId, string Field, IReadOnlyDictionary<string, string> ValuesBySource);
