namespace BrasilCompete.Worker.Domain;

public sealed record SportEvent
{
    /// <summary>Identificador determinístico, atribuído pelo pipeline.</summary>
    public string Id { get; init; } = string.Empty;

    public required Sport Sport { get; init; }

    public required string Competition { get; init; }

    public string? Stage { get; init; }

    public required EventFormat Format { get; init; }

    public required CompetitionScope Scope { get; init; }

    /// <summary>Visão do app, atribuída pelo pipeline.</summary>
    public EventView? View { get; init; }

    public required Schedule Schedule { get; init; }

    public string? Venue { get; init; }

    public required IReadOnlyList<Participant> Participants { get; init; }

    public required IReadOnlyList<SourceReference> Sources { get; init; }

    public Confidence Confidence { get; init; } = Confidence.High;

    /// <summary>Eventos de exemplo da curadoria, usados só para exercitar o fluxo.</summary>
    public bool IsTestData { get; init; }
}
