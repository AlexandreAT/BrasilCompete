using BrasilCompete.Worker.Domain;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

public sealed class CommandDispatcher(
    CollectCommand collect,
    TimeProvider timeProvider,
    ILogger<CommandDispatcher> logger)
{
    private const string Usage = """
        Uso:
          collect --from aaaa-mm-dd --to aaaa-mm-dd [--sources a,b] [--no-cache]
          collect --days N [--sources a,b] [--no-cache]

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
            var today = BrasiliaTime.ToDate(timeProvider.GetUtcNow());

            return args[0] switch
            {
                "collect" => await collect.RunAsync(CollectCommandOptions.Parse(args[1..], today), cancellationToken),
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
