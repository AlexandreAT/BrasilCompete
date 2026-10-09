namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaPageResponse
{
    public string Title { get; init; } = string.Empty;

    public bool Missing { get; init; }

    public IReadOnlyList<LiquipediaRevisionResponse> Revisions { get; init; } = [];

    public string? Content => Revisions.FirstOrDefault()?.Slots?.Main?.Content;
}
