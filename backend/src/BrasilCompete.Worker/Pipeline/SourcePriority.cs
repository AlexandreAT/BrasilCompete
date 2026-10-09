using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Manual;

using Microsoft.Extensions.Options;

namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Prioridade das fontes na deduplicação, configurada em <c>Worker:SourcePriority</c>.
/// A curadoria manual vence todas (o <c>ManualOverride</c> do guia).
/// </summary>
public sealed class SourcePriority(IOptions<WorkerOptions> options)
{
    public int Rank(string source)
    {
        if (source == ManualEventSource.SourceName)
        {
            return -1;
        }

        var index = options.Value.SourcePriority.IndexOf(source);

        return index < 0 ? int.MaxValue : index;
    }

    public int Rank(SportEvent sportEvent) => sportEvent.Sources.Min(source => Rank(source.Source));
}
