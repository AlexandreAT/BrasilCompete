using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations.Manual;

public sealed record ManualEventEntry
{
    /// <summary>Identificador estável da entrada na curadoria.</summary>
    public required string Key { get; init; }

    public bool IsTestData { get; init; }

    public required Sport Sport { get; init; }

    public required string Competition { get; init; }

    public string? Stage { get; init; }

    public required EventFormat Format { get; init; }

    public required CompetitionScope Scope { get; init; }

    public required ManualScheduleEntry Schedule { get; init; }

    public string? Venue { get; init; }

    public required IReadOnlyList<ManualParticipantEntry> Participants { get; init; }

    /// <summary>Página oficial consultada para registrar o evento.</summary>
    public required string SourceUrl { get; init; }

    public string? Note { get; init; }
}
