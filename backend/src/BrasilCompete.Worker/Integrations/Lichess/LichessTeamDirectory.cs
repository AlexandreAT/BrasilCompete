using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Lichess.Contracts;

namespace BrasilCompete.Worker.Integrations.Lichess;

/// <summary>
/// Equipes de um torneio por equipes, a partir da lista de jogadores. Uma equipe em que todos os jogadores são
/// da mesma federação é tratada como seleção (Olimpíada); as demais, como clubes (ligas com estrangeiros).
/// </summary>
public sealed class LichessTeamDirectory
{
    private readonly Dictionary<string, string> teamByPlayer = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string?> federationByTeam = new(StringComparer.Ordinal);

    public LichessTeamDirectory(IEnumerable<LichessPlayerResponse> players)
    {
        foreach (var group in players.Where(player => !string.IsNullOrWhiteSpace(player.Team)).GroupBy(player => player.Team!.Trim()))
        {
            var federations = group.Select(player => player.Fed?.Trim().ToUpperInvariant()).Distinct().ToList();
            federationByTeam[group.Key] = federations is [{ Length: > 0 } federation] ? federation : null;

            foreach (var player in group)
            {
                teamByPlayer.TryAdd(Key(player), group.Key);
            }
        }
    }

    public bool HasTeams => federationByTeam.Count > 0;

    public string? TeamOf(LichessPlayerResponse player) => teamByPlayer.GetValueOrDefault(Key(player));

    public Participant ToTeamParticipant(string team, IEnumerable<LichessPlayerResponse> members)
    {
        var federation = federationByTeam.GetValueOrDefault(team);

        return new Participant
        {
            Name = team,
            Kind = federation is null ? ParticipantKind.Club : ParticipantKind.NationalTeam,
            Country = federation,
            Members = members.Select(LichessMapper.ToParticipant).ToList(),
        };
    }

    private static string Key(LichessPlayerResponse player) =>
        player.FideId is { } fideId ? $"fide:{fideId}" : $"name:{player.Name.Trim()}";
}
