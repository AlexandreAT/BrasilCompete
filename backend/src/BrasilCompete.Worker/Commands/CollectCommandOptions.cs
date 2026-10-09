using System.Globalization;

using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Commands;

/// <summary>
/// Opções do comando <c>collect</c>: a janela (<c>--from</c> e <c>--to</c>, ou <c>--days</c> a partir de hoje),
/// as fontes (<c>--sources a,b</c>) e <c>--no-cache</c>.
/// </summary>
public sealed record CollectCommandOptions(DateWindow Window, IReadOnlySet<string>? Sources, bool NoCache)
{
    public static CollectCommandOptions Parse(IReadOnlyList<string> args, DateOnly today)
    {
        DateOnly? from = null;
        DateOnly? to = null;
        int? days = null;
        HashSet<string>? sources = null;
        var noCache = false;

        for (var index = 0; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--from":
                    from = ParseDate(NextValue(args, ref index));
                    break;
                case "--to":
                    to = ParseDate(NextValue(args, ref index));
                    break;
                case "--days":
                    days = ParseDays(NextValue(args, ref index));
                    break;
                case "--sources":
                    sources = NextValue(args, ref index)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    break;
                case "--no-cache":
                    noCache = true;
                    break;
                default:
                    throw new CommandLineException($"Opção desconhecida: {args[index]}");
            }
        }

        if (days is { } count)
        {
            from ??= today;
            to ??= from.Value.AddDays(count - 1);
        }

        if (from is null || to is null)
        {
            throw new CommandLineException("Informe --from e --to (aaaa-mm-dd) ou --days N.");
        }

        if (to < from)
        {
            throw new CommandLineException("A data de --to precisa ser igual ou posterior à de --from.");
        }

        return new CollectCommandOptions(new DateWindow(from.Value, to.Value), sources, noCache);
    }

    private static string NextValue(IReadOnlyList<string> args, ref int index)
    {
        if (index + 1 >= args.Count)
        {
            throw new CommandLineException($"A opção {args[index]} precisa de um valor.");
        }

        index++;

        return args[index];
    }

    private static DateOnly ParseDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new CommandLineException($"Data inválida: {value} (use aaaa-mm-dd).");

    private static int ParseDays(string value) =>
        int.TryParse(value, CultureInfo.InvariantCulture, out var days) && days > 0
            ? days
            : throw new CommandLineException($"Quantidade de dias inválida: {value}.");
}
