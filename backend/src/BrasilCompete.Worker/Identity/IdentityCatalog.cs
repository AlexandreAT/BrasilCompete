namespace BrasilCompete.Worker.Identity;

/// <summary>Catálogo de atletas ligados ao Brasil, gerado pelo comando <c>identity</c> a partir do Wikidata.</summary>
public sealed record IdentityCatalog
{
    public DateTimeOffset GeneratedAtUtc { get; init; }

    public IReadOnlyList<IdentityProfileSummary> Profiles { get; init; } = [];

    public IReadOnlyList<IdentityRecord> Records { get; init; } = [];
}
