using System.Text.Json;

namespace BrasilCompete.Worker.Tests;

/// <summary>Lê as respostas reais salvas em <c>Fixtures/</c> (os testes nunca usam a internet).</summary>
internal static class Fixtures
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    public static string Read(params string[] path) =>
        File.ReadAllText(Path.Combine([AppContext.BaseDirectory, "Fixtures", .. path]));

    public static T ReadJson<T>(params string[] path) =>
        JsonSerializer.Deserialize<T>(Read(path), Web) ?? throw new InvalidDataException($"Fixture vazia: {string.Join('/', path)}");
}
