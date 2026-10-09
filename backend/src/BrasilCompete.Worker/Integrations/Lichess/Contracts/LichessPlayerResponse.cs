namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>Jogador de uma transmissão ou de uma partida, com a federação (<c>fed</c>) e o ID FIDE.</summary>
public sealed record LichessPlayerResponse
{
    public string Name { get; init; } = string.Empty;

    public string? Title { get; init; }

    public int? Rating { get; init; }

    public long? FideId { get; init; }

    public string? Fed { get; init; }

    /// <summary>Equipe do jogador em competições por equipes (Olimpíada, ligas de clubes).</summary>
    public string? Team { get; init; }
}
