namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaQueryResultResponse
{
    public IReadOnlyList<LiquipediaPageResponse> Pages { get; init; } = [];

    /// <summary>Títulos ajustados pelo MediaWiki (primeira letra maiúscula, por exemplo).</summary>
    public IReadOnlyList<LiquipediaRedirectResponse> Normalized { get; init; } = [];

    public IReadOnlyList<LiquipediaRedirectResponse> Redirects { get; init; } = [];

    public IReadOnlyList<LiquipediaPageResponse> Allpages { get; init; } = [];
}
