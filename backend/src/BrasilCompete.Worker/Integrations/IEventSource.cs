using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Integrations;

/// <summary>
/// Um adapter por fonte. Devolve eventos já no modelo interno; o formato externo não sai da integração.
/// </summary>
public interface IEventSource
{
    string Name { get; }

    bool IsEnabled { get; }

    Task<SourceCollection> CollectAsync(DateWindow window, CancellationToken cancellationToken);
}
