namespace BrasilCompete.Worker.Integrations.Jolpica.Contracts;

/// <summary>Uma etapa do calendário, com o horário (UTC) da corrida e de cada sessão.</summary>
public sealed record JolpicaRaceResponse
{
    public string Season { get; init; } = string.Empty;

    public string Round { get; init; } = string.Empty;

    public string? Url { get; init; }

    public string RaceName { get; init; } = string.Empty;

    public JolpicaCircuitResponse? Circuit { get; init; }

    public string Date { get; init; } = string.Empty;

    public string? Time { get; init; }

    public JolpicaSessionResponse? FirstPractice { get; init; }

    public JolpicaSessionResponse? SecondPractice { get; init; }

    public JolpicaSessionResponse? ThirdPractice { get; init; }

    public JolpicaSessionResponse? Qualifying { get; init; }

    public JolpicaSessionResponse? Sprint { get; init; }

    public JolpicaSessionResponse? SprintQualifying { get; init; }
}
