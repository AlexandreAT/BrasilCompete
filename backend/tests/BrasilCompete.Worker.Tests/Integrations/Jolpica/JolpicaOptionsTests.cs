using BrasilCompete.Worker.Integrations.Jolpica;

using Microsoft.Extensions.Configuration;

namespace BrasilCompete.Worker.Tests.Integrations.Jolpica;

public sealed class JolpicaOptionsTests
{
    [Fact]
    public void Bind_SessionsFromConfiguration_AreNotRepeated()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sessions:0"] = "Race",
                ["Sessions:1"] = "Qualifying",
                ["Sessions:2"] = "Sprint",
                ["Sessions:3"] = "SprintQualifying",
            })
            .Build();

        var options = configuration.Get<JolpicaOptions>()!;

        Assert.Equal(
            [JolpicaSession.Race, JolpicaSession.Qualifying, JolpicaSession.Sprint, JolpicaSession.SprintQualifying],
            options.EffectiveSessions);
    }

    [Fact]
    public void EffectiveSessions_WithoutConfiguration_UsesTheCompetitiveSessions() =>
        Assert.Equal(4, new JolpicaOptions().EffectiveSessions.Count);
}
