using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrasilCompete.Worker.Integrations.Manual;

public static class ManualRegistration
{
    public static IServiceCollection AddManualSource(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ManualOptions>(configuration.GetSection(ManualOptions.SectionName));
        services.AddSingleton<IEventSource, ManualEventSource>();

        return services;
    }
}
