namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>Lutador como aparece no card: nome e, quando houver, o artigo da Wikipedia.</summary>
public sealed record UfcFighter(string Name, string? WikipediaTitle);
