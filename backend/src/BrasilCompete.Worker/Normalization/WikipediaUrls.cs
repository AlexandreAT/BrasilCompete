namespace BrasilCompete.Worker.Normalization;

public static class WikipediaUrls
{
    private const string EnglishPrefix = "en.wikipedia.org/wiki/";

    /// <summary>
    /// Título do artigo da Wikipedia em inglês a partir do link, como
    /// <c>https://en.wikipedia.org/wiki/Gabriel_Bortoleto</c> → <c>Gabriel Bortoleto</c>.
    /// </summary>
    public static string? TryGetEnglishTitle(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var index = url.IndexOf(EnglishPrefix, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return null;
        }

        var title = url[(index + EnglishPrefix.Length)..].Split('#', '?')[0];

        return title.Length == 0 ? null : Uri.UnescapeDataString(title).Replace('_', ' ');
    }
}
