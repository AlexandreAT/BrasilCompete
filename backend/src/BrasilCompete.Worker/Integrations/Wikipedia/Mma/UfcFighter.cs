namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>
/// Lutador como aparece no card: nome e, quando houver, o artigo da Wikipedia. O país (código de três letras)
/// vem da bandeira na lista do elenco atual do UFC, quando o lutador está nela.
/// </summary>
public sealed record UfcFighter(string Name, string? WikipediaTitle, string? Country = null);
