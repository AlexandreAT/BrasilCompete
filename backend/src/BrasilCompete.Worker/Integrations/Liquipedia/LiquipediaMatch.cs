namespace BrasilCompete.Worker.Integrations.Liquipedia;

/// <summary>
/// Uma partida lida de uma página <c>Match:</c>. A data local vem com a sigla do fuso; sem sigla, sem horário.
/// Um oponente ainda indefinido fica <c>null</c>.
/// </summary>
public sealed record LiquipediaMatch(
    string PageTitle,
    string BracketId,
    DateOnly? Date,
    TimeOnly? LocalTime,
    string? TimezoneAbbreviation,
    string? FirstTeam,
    string? SecondTeam);
