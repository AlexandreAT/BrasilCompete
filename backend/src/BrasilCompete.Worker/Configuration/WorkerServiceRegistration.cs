using BrasilCompete.Worker.Commands;
using BrasilCompete.Worker.History;
using BrasilCompete.Worker.Http;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Manual;
using BrasilCompete.Worker.Output;
using BrasilCompete.Worker.Pipeline;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Configuration;

public static class WorkerServiceRegistration
{
    public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WorkerOptions>(configuration.GetSection(WorkerOptions.SectionName));

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

        services.AddSingleton<CollectCommand>();
        services.AddSingleton<CommandDispatcher>();

        services.AddManualSource(configuration);

        return services;
    }
}
