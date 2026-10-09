namespace BrasilCompete.Worker.Domain;

/// <summary>
/// Janela de coleta, com datas inclusivas no horário de Brasília.
/// </summary>
public sealed record DateWindow(DateOnly From, DateOnly To)
{
    /// <summary>
    /// Eventos "A confirmar" não têm data e por isso nunca ficam fora da janela.
    /// </summary>
    public bool Overlaps(Schedule schedule)
    {
        var span = schedule.GetDateSpan();

        return span is null || (span.Value.First <= To && span.Value.Last >= From);
    }

    public override string ToString() => $"{From:yyyy-MM-dd} a {To:yyyy-MM-dd}";
}
