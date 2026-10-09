namespace BrasilCompete.Worker.Validation;

/// <summary>
/// Uma linha do gabarito (<c>validation/reference-*.csv</c>): um evento conferido numa fonte oficial.
/// Sem data, a linha só registra que a entidade não teve evento na janela e fica fora das métricas.
/// </summary>
public sealed record ReferenceRow(
    int Line,
    string Sport,
    string Competition,
    string Stage,
    IReadOnlyList<string> Participants,
    string View,
    DateOnly? Date,
    TimeOnly? Time,
    string ExpectedPrecision,
    string SourceUrl,
    string Note);
