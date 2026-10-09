using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Integrations.Jolpica;

public static class JolpicaRegistration
{
    public static IServiceCollection AddJolpicaSource(
        this IServiceCollection services,
        IConfiguration configuration,
        WorkerOptions workerOptions)
    {
        var section = configuration.GetSection(JolpicaOptions.SectionName);
        var options = section.Get<JolpicaOptions>() ?? new JolpicaOptions();

        services.Configure<JolpicaOptions>(section);
        services.AddSourceHttpClient(JolpicaClient.ClientName, options.Http, workerOptions);
        services.AddSingleton<JolpicaClient>();
        services.AddSingleton<IEventSource, JolpicaEventSource>();

        return services;
    }
}
