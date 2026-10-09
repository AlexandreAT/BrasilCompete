using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Tests;

/// <summary>Atalhos para montar eventos e participantes nos testes.</summary>
internal static class TestEvents
{
    public static readonly DateTimeOffset RetrievedAt = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    public static Participant Athlete(string name, string? country = null, BrazilianReason? reason = null) => new()
    {
        Name = name,
        Kind = ParticipantKind.Athlete,
        Country = country,
        IsBrazilian = reason is not null,
        BrazilianReason = reason,
        DecidedBy = reason is null ? null : IdentityLayer.Manual,
    };

    public static Participant Team(string name, ParticipantKind kind, string country, params Participant[] members) => new()
    {
        Name = name,
        Kind = kind,
        Country = country,
        Members = members,
    };

    public static SportEvent Matchup(
        Sport sport,
        Schedule schedule,
        string source,
        params Participant[] participants) => new()
    {
        Sport = sport,
        Competition = "Competição de teste",
        Format = EventFormat.Matchup,
        Scope = CompetitionScope.International,
        Schedule = schedule,
        Participants = participants,
        Sources = [Source(source)],
    };

    public static SportEvent Participation(
        Sport sport,
        Schedule schedule,
        string source,
        string? stage,
        params Participant[] participants) => new()
    {
        Sport = sport,
        Competition = "Competição de teste",
        Stage = stage,
        Format = EventFormat.Participation,
        Scope = CompetitionScope.International,
        Schedule = schedule,
        Participants = participants,
        Sources = [Source(source)],
    };

    public static SourceReference Source(string source, string? externalId = null) => new()
    {
        Source = source,
        Url = $"https://example.org/{source}",
        License = "Teste",
        RetrievedAtUtc = RetrievedAt,
        ExternalId = externalId,
    };

    public static Schedule At(int year, int month, int day, int hourUtc) =>
        Schedule.AtTime(new DateTimeOffset(year, month, day, hourUtc, 0, 0, TimeSpan.Zero), "Etc/UTC");
}
