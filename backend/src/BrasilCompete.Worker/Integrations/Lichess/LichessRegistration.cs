using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Integrations.Lichess;

public static class LichessRegistration
{
    public static IServiceCollection AddLichessSource(
        this IServiceCollection services,
        IConfiguration configuration,
        WorkerOptions workerOptions)
    {
        var section = configuration.GetSection(LichessOptions.SectionName);
        var options = section.Get<LichessOptions>() ?? new LichessOptions();

        services.Configure<LichessOptions>(section);
        services.AddSourceHttpClient(LichessClient.ClientName, options.Http, workerOptions);
        services.AddSingleton<LichessClient>();
        services.AddSingleton<IEventSource, LichessEventSource>();

        return services;
    }
}
