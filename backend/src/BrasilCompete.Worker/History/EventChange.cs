namespace BrasilCompete.Worker.History;

public sealed record EventChange(DateTimeOffset AtUtc, string Field, string? From, string? To);
