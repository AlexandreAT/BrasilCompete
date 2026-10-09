using System.Text;
using System.Text.RegularExpressions;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

/// <summary>
/// Leitura mínima de wikitext: predefinições de primeiro nível, links internos e texto sem marcação.
/// Não é um parser completo; cobre o que aparece nos parâmetros das predefinições de partida.
/// </summary>
public static partial class WikitextReader
{
    public static IReadOnlyList<WikitextTemplate> ReadTemplates(string wikitext)
    {
        wikitext = CommentPattern().Replace(wikitext, string.Empty);
        var templates = new List<WikitextTemplate>();
        var depth = 0;
        var start = -1;
        var index = 0;

        while (index < wikitext.Length - 1)
        {
            if (IsPair(wikitext, index, '{'))
            {
                if (depth == 0)
                {
                    start = index + 2;
                }

                depth++;
                index += 2;
            }
            else if (IsPair(wikitext, index, '}') && depth > 0)
            {
                depth--;
                index += 2;

                if (depth == 0)
                {
                    templates.Add(ParseTemplate(wikitext[start..(index - 2)]));
                }
            }
            else
            {
                index++;
            }
        }

        return templates;
    }

    /// <summary>Todas as predefinições, inclusive as aninhadas nos parâmetros de outras.</summary>
    public static IEnumerable<WikitextTemplate> ReadAllTemplates(string wikitext)
    {
        foreach (var template in ReadTemplates(wikitext))
        {
            yield return template;

            foreach (var nested in template.Arguments.SelectMany(ReadAllTemplates))
            {
                yield return nested;
            }
        }
    }

    public static WikitextLink? ReadFirstLink(string wikitext)
    {
        var match = LinkPattern().Match(wikitext);

        if (!match.Success)
        {
            return null;
        }

        var target = match.Groups["target"].Value.Trim();
        var text = match.Groups["text"].Success ? match.Groups["text"].Value.Trim() : target;

        return new WikitextLink(target, text);
    }

    /// <summary>Texto legível: links viram o texto, predefinições e referências somem.</summary>
    public static string ToPlainText(string wikitext)
    {
        var withoutReferences = ReferencePattern().Replace(wikitext, string.Empty);
        var withoutTemplates = RemoveTemplates(withoutReferences);
        var withLinksAsText = LinkPattern().Replace(withoutTemplates, match =>
            match.Groups["text"].Success ? match.Groups["text"].Value : match.Groups["target"].Value);
        var withoutTags = TagPattern().Replace(withLinksAsText, string.Empty);

        return SpacesPattern().Replace(withoutTags.Replace("'''", string.Empty).Replace("''", string.Empty), " ").Trim(' ', ',', '\n');
    }

    private static WikitextTemplate ParseTemplate(string body)
    {
        var parts = SplitTopLevel(body);

        return new WikitextTemplate(parts[0].Trim(), parts.Skip(1).ToList());
    }

    /// <summary>Separa por <c>|</c> sem quebrar predefinições e links aninhados.</summary>
    private static List<string> SplitTopLevel(string body)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var depth = 0;
        var index = 0;

        while (index < body.Length)
        {
            if (IsPair(body, index, '{') || IsPair(body, index, '['))
            {
                depth++;
                current.Append(body, index, 2);
                index += 2;
            }
            else if (IsPair(body, index, '}') || IsPair(body, index, ']'))
            {
                depth--;
                current.Append(body, index, 2);
                index += 2;
            }
            else if (body[index] == '|' && depth == 0)
            {
                parts.Add(current.ToString());
                current.Clear();
                index++;
            }
            else
            {
                current.Append(body[index]);
                index++;
            }
        }

        parts.Add(current.ToString());

        return parts;
    }

    private static string RemoveTemplates(string wikitext)
    {
        var builder = new StringBuilder();
        var depth = 0;
        var index = 0;

        while (index < wikitext.Length)
        {
            if (IsPair(wikitext, index, '{'))
            {
                depth++;
                index += 2;
            }
            else if (IsPair(wikitext, index, '}') && depth > 0)
            {
                depth--;
                index += 2;
            }
            else
            {
                if (depth == 0)
                {
                    builder.Append(wikitext[index]);
                }

                index++;
            }
        }

        return builder.ToString();
    }

    private static bool IsPair(string text, int index, char character) =>
        index < text.Length - 1 && text[index] == character && text[index + 1] == character;

    [GeneratedRegex(@"\[\[(?<target>[^\]\|]+)(\|(?<text>[^\]]+))?\]\]")]
    private static partial Regex LinkPattern();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex CommentPattern();

    [GeneratedRegex(@"<ref[^>]*/>|<ref[^>]*>.*?</ref>", RegexOptions.Singleline)]
    private static partial Regex ReferencePattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesPattern();
}
