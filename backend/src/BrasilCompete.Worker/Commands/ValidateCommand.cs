using System.Text.Json;

using BrasilCompete.Worker.Configuration;
using BrasilCompete.Worker.Output;
using BrasilCompete.Worker.Serialization;
using BrasilCompete.Worker.Validation;

using Microsoft.Extensions.Logging;

namespace BrasilCompete.Worker.Commands;

/// <summary>
/// Compara um gabarito (<c>--reference</c>, padrão <c>validation/reference-past.csv</c>) com os eventos de uma
/// execução (<c>--events</c>, padrão <c>output/latest/events.json</c>) e grava o resultado em
/// <c>output/validation/&lt;gabarito&gt;.md</c>.
/// </summary>
public sealed class ValidateCommand(BackendPaths paths, ILogger<ValidateCommand> logger)
{
    public async Task<int> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        var reference = paths.Resolve(OptionValue(args, "--reference") ?? Path.Combine("validation", "reference-past.csv"));
        var eventsPath = paths.Resolve(OptionValue(args, "--events") ?? Path.Combine("output", "latest", "events.json"));

        if (!File.Exists(reference) || !File.Exists(eventsPath))
        {
            throw new CommandLineException($"Arquivo não encontrado: {(File.Exists(reference) ? eventsPath : reference)}");
        }

        var rows = ReferenceCsvReader.Read(await File.ReadAllTextAsync(reference, cancellationToken));
        await using var stream = File.OpenRead(eventsPath);
        var events = await JsonSerializer.DeserializeAsync<EventsFile>(stream, JsonDefaults.Options, cancellationToken)
            ?? throw new CommandLineException($"Arquivo de eventos inválido: {eventsPath}");

        var matches = ReferenceMatcher.Match(rows, events.Events.Where(sportEvent => !sportEvent.IsTestData).ToList());
        var name = Path.GetFileNameWithoutExtension(reference);
        var directory = Path.Combine(paths.OutputDirectory, "validation");
        Directory.CreateDirectory(directory);

        var output = Path.Combine(directory, $"{name}.md");
        await File.WriteAllTextAsync(output, ValidationReportBuilder.Build(name, events.RunId, matches), cancellationToken);

        logger.LogInformation(
            "Gabarito {Name}: {Found} de {Total} encontrados ({Skipped} linhas sem data ignoradas). Resultado em {Path}",
            name,
            matches.Count(match => match.Found),
            matches.Count,
            rows.Count - matches.Count,
            output);

        return 0;
    }

    private static string? OptionValue(IReadOnlyList<string> args, string option)
    {
        var index = args.ToList().IndexOf(option);

        if (index < 0)
        {
            return null;
        }

        return index + 1 < args.Count ? args[index + 1] : throw new CommandLineException($"A opção {option} precisa de um valor.");
    }
}
