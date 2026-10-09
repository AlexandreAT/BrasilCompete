namespace BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;

/// <summary>
/// Uma predefinição encontrada no HTML do Parsoid, com os parâmetros em wikitext. <see cref="Heading"/> é o título
/// de seção mais próximo; <see cref="TopHeading"/>, o título de nível 2 (ex.: a prova, como "Men's singles").
/// </summary>
public sealed record ParsoidTemplate(
    string Name,
    IReadOnlyDictionary<string, string> Parameters,
    string? Heading,
    int? HeadingYear,
    string? TopHeading = null);
