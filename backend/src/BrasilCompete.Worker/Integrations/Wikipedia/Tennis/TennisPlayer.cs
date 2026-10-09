namespace BrasilCompete.Worker.Integrations.Wikipedia.Tennis;

/// <summary>Jogador numa chave: nome, artigo da Wikipedia e país da bandeira (código de três letras).</summary>
public sealed record TennisPlayer(string Name, string? WikipediaTitle, string? Country);
