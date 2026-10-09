namespace BrasilCompete.Worker.Integrations.Lichess.Contracts;

/// <summary>
/// Detalhe de um torneio (<c>/api/broadcast/{id}</c>): todas as rodadas e, se houver, o grupo completo.
/// Nas listas, <c>group</c> é só o nome; aqui é um objeto com todas as transmissões do grupo.
/// </summary>
public sealed record LichessTourDetailsResponse
{
    public LichessTourResponse Tour { get; init; } = new();

    public IReadOnlyList<LichessRoundInfoResponse> Rounds { get; init; } = [];

    public LichessGroupResponse? Group { get; init; }
}
