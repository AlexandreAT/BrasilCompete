namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>Um evento da "List of UFC events": nome, artigo, data local e local.</summary>
public sealed record UfcEventListing(string Name, string? Title, DateOnly Date, string? Venue, string? Location);
