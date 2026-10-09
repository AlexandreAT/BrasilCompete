using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity;

/// <summary>
/// Decide quem é brasileiro (plano, seção 4.1), em camadas: o que a curadoria decidiu vale primeiro,
/// depois a nacionalidade informada pela própria fonte.
/// </summary>
public sealed class IdentityResolver
{
    public const string BrazilCountryCode = "BRA";

    public SportEvent Resolve(SportEvent sportEvent) => sportEvent with
    {
        Participants = sportEvent.Participants.Select(Resolve).ToList(),
    };

    public static bool HasBrazilian(SportEvent sportEvent) =>
        sportEvent.Participants.Any(participant => participant.IsBrazilian || participant.Members.Any(member => member.IsBrazilian));

    private static Participant Resolve(Participant participant)
    {
        var members = participant.Members.Select(Resolve).ToList();
        var resolved = participant with { Members = members };

        if (participant.DecidedBy is not null)
        {
            return resolved;
        }

        if (string.Equals(participant.Country, BrazilCountryCode, StringComparison.OrdinalIgnoreCase))
        {
            return resolved with
            {
                IsBrazilian = true,
                BrazilianReason = ReasonFor(participant.Kind),
                DecidedBy = IdentityLayer.Source,
            };
        }

        if (participant.Kind == ParticipantKind.Pair && members.Count > 0 && members.All(member => member.IsBrazilian))
        {
            return resolved with
            {
                IsBrazilian = true,
                BrazilianReason = Domain.BrazilianReason.RepresentsBrazil,
                DecidedBy = members[0].DecidedBy,
            };
        }

        return resolved;
    }

    private static BrazilianReason ReasonFor(ParticipantKind kind) => kind switch
    {
        ParticipantKind.NationalTeam => Domain.BrazilianReason.BrazilianNationalTeam,
        ParticipantKind.Club => Domain.BrazilianReason.BrazilianClub,
        ParticipantKind.Organization => Domain.BrazilianReason.BrazilianOrganization,
        _ => Domain.BrazilianReason.RepresentsBrazil,
    };
}
