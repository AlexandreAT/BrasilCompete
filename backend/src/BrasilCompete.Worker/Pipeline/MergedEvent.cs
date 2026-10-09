namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Um grupo de eventos que virou um só. <see cref="AcrossSources"/> diferencia a junção entre fontes
/// (o objetivo da deduplicação) de uma repetição dentro da mesma fonte.
/// </summary>
public sealed record MergedEvent(string EventId, IReadOnlyList<string> Sources, bool AcrossSources);
