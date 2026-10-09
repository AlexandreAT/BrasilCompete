namespace BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;

/// <summary>Uma predefinição encontrada no HTML do Parsoid, com os parâmetros em wikitext.</summary>
public sealed record ParsoidTemplate(
    string Name,
    IReadOnlyDictionary<string, string> Parameters,
    string? Heading,
    int? HeadingYear);
