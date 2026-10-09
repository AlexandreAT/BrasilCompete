using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Validation;

/// <summary>
/// Resultado de uma linha do gabarito: o evento encontrado (ou nenhum) e se a data e o horário batem.
/// <c>null</c> em <see cref="DateCorrect"/> ou <see cref="TimeCorrect"/> quando não há o que comparar
/// (evento só com período, ou gabarito sem horário). <see cref="NameMismatch"/> indica que só parte dos
/// participantes bateu pelo nome (ex.: "Dooho Choi" e "Choi Doo-ho"), mas o evento era o único candidato.
/// </summary>
public sealed record ReferenceMatch(ReferenceRow Row, SportEvent? Event, bool? DateCorrect, bool? TimeCorrect, bool NameMismatch = false)
{
    public bool Found => Event is not null;

    /// <summary>O gabarito tem horário oficial, mas o evento veio só com data, período ou "a confirmar".</summary>
    public bool LostTime => Row.Time is not null && Event is { Schedule.Precision: not SchedulePrecision.DateAndTime };
}
