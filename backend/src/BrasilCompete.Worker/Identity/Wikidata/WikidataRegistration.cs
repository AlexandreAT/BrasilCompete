using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Identity.Wikidata;

public static class WikidataRegistration
{
    public static IServiceCollection AddWikidataIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        WorkerOptions workerOptions)
    {
        var section = configuration.GetSection(WikidataOptions.SectionName);
        var options = section.Get<WikidataOptions>() ?? new WikidataOptions();

        services.Configure<WikidataOptions>(section);
        services.AddSourceHttpClient(WikidataOptions.ClientName, options.Http, workerOptions);
        services.AddSingleton<WikidataClient>();
        services.AddSingleton<WikidataCatalogBuilder>();

        services.AddSingleton<IdentityCatalogStore>();
        services.AddSingleton(provider => IdentityIndexLoader.Load(
            provider.GetRequiredService<IdentityCatalogStore>(),
            provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentityIndexLoader))));

        return services;
    }
}
