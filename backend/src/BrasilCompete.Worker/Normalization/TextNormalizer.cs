using System.Globalization;
using System.Text;

namespace BrasilCompete.Worker.Normalization;

public static class TextNormalizer
{
    /// <summary>Remove acentos: "São Paulo" vira "Sao Paulo".</summary>
    public static string RemoveDiacritics(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Gera um trecho de identificador: "Gabriel Bortoleto" vira "gabriel-bortoleto".</summary>
    public static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);
        var pendingSeparator = false;

        foreach (var character in RemoveDiacritics(value).ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                if (pendingSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(character);
                pendingSeparator = false;
            }
            else
            {
                pendingSeparator = true;
            }
        }

        return builder.ToString();
    }

    /// <summary>Chave de comparação de nomes, sem acentos, caixa, espaços ou pontuação.</summary>
    public static string NormalizeName(string value) =>
        Slugify(value).Replace("-", string.Empty, StringComparison.Ordinal);
}
