namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>Time pela página da Liquipedia: o nome da página e o país do infobox.</summary>
public sealed record LiquipediaTeam(string Name, string? Location);
