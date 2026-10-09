using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BrasilCompete.Worker.Validation;

/// <summary>
/// Lê o gabarito em CSV (vírgula como separador, campos com vírgula entre aspas duplas e aspas internas dobradas).
/// As colunas são encontradas pelo nome do cabeçalho.
/// </summary>
public static partial class ReferenceCsvReader
{
    public static IReadOnlyList<ReferenceRow> Read(string csv)
    {
        var records = ParseRecords(csv);

        if (records.Count == 0)
        {
            return [];
        }

        var header = records[0].Select(name => name.Trim()).ToList();
        var rows = new List<ReferenceRow>();

        for (var index = 1; index < records.Count; index++)
        {
            var fields = records[index];

            if (fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            string Field(string name) => header.IndexOf(name) is var column and >= 0 && column < fields.Count ? fields[column].Trim() : string.Empty;

            rows.Add(new ReferenceRow(
                index + 1,
                Field("modalidade"),
                Field("competicao"),
                Field("fase"),
                SplitParticipants(Field("participantes")),
                Field("visao"),
                DateOnly.TryParseExact(Field("data"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null,
                TimeOnly.TryParseExact(Field("hora_brasilia"), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time : null,
                Field("precisao_esperada"),
                Field("url_fonte_oficial"),
                Field("observacao")));
        }

        return rows;
    }

    /// <summary>
    /// "Austrália x Brasil" vira dois participantes; "Gabriel Bortoleto (Audi)" fica sem o parêntese; na visão
    /// Indivíduos ("Vinícius Júnior (Real Madrid) — Real Madrid x Espanyol"), contam o atleta e os dois times.
    /// </summary>
    public static IReadOnlyList<string> SplitParticipants(string value)
    {
        var parts = value.Split(" — ", 2, StringSplitOptions.TrimEntries);
        var names = parts.Length == 2 ? [parts[0], .. parts[1].Split(" x ", StringSplitOptions.TrimEntries)] : value.Split(" x ", StringSplitOptions.TrimEntries);

        return names
            .Select(name => ParenthesisPattern().Replace(name, string.Empty).Trim())
            .Where(name => name.Length > 0)
            .ToList();
    }

    private static List<List<string>> ParseRecords(string csv)
    {
        var records = new List<List<string>>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var index = 0;

        while (index < csv.Length)
        {
            var character = csv[index];

            if (quoted)
            {
                if (character == '"' && index + 1 < csv.Length && csv[index + 1] == '"')
                {
                    field.Append('"');
                    index += 2;
                    continue;
                }

                quoted = character != '"';

                if (quoted)
                {
                    field.Append(character);
                }

                index++;
                continue;
            }

            switch (character)
            {
                case '"':
                    quoted = true;
                    break;
                case ',':
                    fields.Add(field.ToString());
                    field.Clear();
                    break;
                case '\n':
                    fields.Add(field.ToString().TrimEnd('\r'));
                    field.Clear();
                    records.Add(fields);
                    fields = [];
                    break;
                default:
                    field.Append(character);
                    break;
            }

            index++;
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString().TrimEnd('\r'));
            records.Add(fields);
        }

        return records;
    }

    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex ParenthesisPattern();
}
