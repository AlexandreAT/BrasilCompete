namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>Uma página lida da Liquipedia: o título final (depois de redirecionamentos) e o wikitext.</summary>
public sealed record LiquipediaPage(string Title, string Content);
