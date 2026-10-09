using BrasilCompete.Worker.Commands;
using BrasilCompete.Worker.History;
using BrasilCompete.Worker.Http;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Identity.Wikidata;
using BrasilCompete.Worker.Integrations.Jolpica;
using BrasilCompete.Worker.Integrations.Lichess;
using BrasilCompete.Worker.Integrations.Liquipedia;
using BrasilCompete.Worker.Integrations.Manual;
using BrasilCompete.Worker.Integrations.Wikipedia;
using BrasilCompete.Worker.Output;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Configuration;

public static class WorkerServiceRegistration
{
    public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var workerSection = configuration.GetSection(WorkerOptions.SectionName);
        var workerOptions = workerSection.Get<WorkerOptions>() ?? new WorkerOptions();
        services.Configure<WorkerOptions>(workerSection);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(_ => BackendPaths.Discover());

        services.AddSingleton<HttpResponseCache>();
        services.AddSingleton<RequestMetrics>();
        services.AddSingleton<PolitenessRegistry>();

        services.AddSingleton<IdentityResolver>();
        services.AddSingleton<ViewClassifier>();
        services.AddSingleton<SourcePriority>();
        services.AddSingleton<EventDeduplicator>();
        services.AddSingleton<EventPipeline>();

        services.AddSingleton<EventHistoryStore>();
        services.AddSingleton<HistoryUpdater>();
        services.AddSingleton<OutputWriter>();

        services.AddSingleton<SourceRunner>();
        services.AddSingleton<CollectCommand>();
        services.AddSingleton<IdentityCommand>();
        services.AddSingleton<ValidateCommand>();
        services.AddSingleton<CommandDispatcher>();

        services.AddWikidataIdentity(configuration, workerOptions);
        services.AddManualSource(configuration);
        services.AddJolpicaSource(configuration, workerOptions);
        services.AddLichessSource(configuration, workerOptions);
        services.AddWikipediaSources(configuration, workerOptions);
        services.AddLiquipediaSource(configuration, workerOptions);

        return services;
    }
}
