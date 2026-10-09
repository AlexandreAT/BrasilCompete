using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Integrations.Wikipedia;

public static class WikipediaRegistration
{
    public static IServiceCollection AddWikipediaSources(
        this IServiceCollection services,
        IConfiguration configuration,
        WorkerOptions workerOptions)
    {
        var section = configuration.GetSection(WikipediaOptions.SectionName);
        var options = section.Get<WikipediaOptions>() ?? new WikipediaOptions();

        services.Configure<WikipediaOptions>(section);
        services.AddSourceHttpClient(WikipediaFootballEventSource.SourceName, options.Http, workerOptions);
        services.AddSourceHttpClient(WikipediaUfcEventSource.SourceName, options.Http, workerOptions);
        services.AddSourceHttpClient(WikipediaTennisEventSource.SourceName, options.Http, workerOptions);
        services.AddSingleton<WikipediaClient>();
        services.AddSingleton<IEventSource, WikipediaFootballEventSource>();
        services.AddSingleton<IEventSource, WikipediaUfcEventSource>();
        services.AddSingleton<IEventSource, WikipediaTennisEventSource>();

        return services;
    }
}
