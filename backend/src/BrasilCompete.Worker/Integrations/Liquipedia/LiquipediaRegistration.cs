using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Integrations.Liquipedia;

public static class LiquipediaRegistration
{
    public static IServiceCollection AddLiquipediaSource(
        this IServiceCollection services,
        IConfiguration configuration,
        WorkerOptions workerOptions)
    {
        var section = configuration.GetSection(LiquipediaOptions.SectionName);
        var options = section.Get<LiquipediaOptions>() ?? new LiquipediaOptions();

        services.Configure<LiquipediaOptions>(section);
        services.AddSourceHttpClient(LiquipediaClient.ClientName, options.Http, workerOptions);
        services.AddSingleton<LiquipediaClient>();
        services.AddSingleton<IEventSource, LiquipediaEventSource>();

        return services;
    }
}
