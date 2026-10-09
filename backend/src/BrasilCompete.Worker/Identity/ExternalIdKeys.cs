namespace BrasilCompete.Worker.Identity;

/// <summary>Chaves de <c>Participant.ExternalIds</c> compartilhadas entre as integrações e a identidade.</summary>
public static class ExternalIdKeys
{
    public const string Wikidata = "wikidata";

    /// <summary>Título do artigo na Wikipedia em inglês, como aparece nos links das predefinições.</summary>
    public const string EnglishWikipedia = "enwiki";
}
