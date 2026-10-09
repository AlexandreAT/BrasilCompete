namespace BrasilCompete.Worker.Integrations.Liquipedia.Contracts;

public sealed record LiquipediaRedirectResponse
{
    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;
}
