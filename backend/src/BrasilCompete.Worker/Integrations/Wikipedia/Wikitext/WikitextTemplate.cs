namespace BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

/// <summary>Uma predefinição de primeiro nível dentro de um trecho de wikitext, como <c>{{fbaicon|ARG}}</c>.</summary>
public sealed record WikitextTemplate(string Name, IReadOnlyList<string> Arguments)
{
    public string? Argument(int index) => index < Arguments.Count ? Arguments[index].Trim() : null;

    /// <summary>Valor de um parâmetro nomeado (<c>|chave=valor</c>), ou <c>null</c>.</summary>
    public string? Named(string key)
    {
        foreach (var argument in Arguments)
        {
            var separator = argument.IndexOf('=', StringComparison.Ordinal);

            if (separator > 0 && argument[..separator].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return argument[(separator + 1)..].Trim();
            }
        }

        return null;
    }
}
