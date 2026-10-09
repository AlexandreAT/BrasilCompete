using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BrasilCompete.Worker.Serialization;

/// <summary>
/// Formato dos arquivos do worker (curadoria, saídas e histórico): camelCase, enums como texto e datas em UTC.
/// </summary>
public static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new UtcDateTimeOffsetConverter());
        options.MakeReadOnly(populateMissingResolver: true);

        return options;
    }
}
