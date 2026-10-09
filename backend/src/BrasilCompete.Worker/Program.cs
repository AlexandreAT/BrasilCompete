using BrasilCompete.Worker.Commands;
using BrasilCompete.Worker.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory,
});

// Os User Secrets são carregados em qualquer ambiente, não só em Development (plano, seção 9.1).
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddWorker(builder.Configuration);

using var host = builder.Build();
using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

return await host.Services.GetRequiredService<CommandDispatcher>().RunAsync(args, cancellation.Token);
