using BrasilCompete.Worker.Domain;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>Escolhe o comando pela linha de comando. Cada comando só é criado quando é usado.</summary>
public sealed class CommandDispatcher(
    IServiceProvider services,
    TimeProvider timeProvider,
    ILogger<CommandDispatcher> logger)
{
    private const string Usage = """
        Uso:
          collect --from aaaa-mm-dd --to aaaa-mm-dd [--sources a,b] [--no-cache]
          collect --days N [--sources a,b] [--no-cache]
          identity            gera o catálogo de identidade a partir do Wikidata

        Exemplo:
          dotnet run --project backend/src/BrasilCompete.Worker -- collect --from 2026-09-01 --to 2026-09-30
        """;

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            Console.WriteLine(Usage);

            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            return args[0] switch
            {
                "collect" => await services.GetRequiredService<CollectCommand>().RunAsync(
                    CollectCommandOptions.Parse(args[1..], BrasiliaTime.ToDate(timeProvider.GetUtcNow())),
                    cancellationToken),
                "identity" => await services.GetRequiredService<IdentityCommand>().RunAsync(cancellationToken),
                _ => throw new CommandLineException($"Comando desconhecido: {args[0]}"),
            };
        }
        catch (CommandLineException exception)
        {
            logger.LogError("{Message}", exception.Message);
            Console.WriteLine(Usage);

            return 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Execução cancelada.");

            return 130;
        }
    }
}
