using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity;

/// <summary>
/// Decide quem é brasileiro (plano, seção 4.1), em camadas: a decisão da curadoria vale primeiro, depois a
/// nacionalidade informada pela própria fonte e, por fim, o Wikidata (nascimento e país esportivo).
/// </summary>
public sealed class IdentityResolver(IdentityIndex index)
{
    public const string BrazilCountryCode = "BRA";

    public SportEvent Resolve(SportEvent sportEvent) => sportEvent with
    {
        Participants = sportEvent.Participants.Select(participant => Resolve(participant, sportEvent.Sport)).ToList(),
    };

    public static bool HasBrazilian(SportEvent sportEvent) =>
        sportEvent.Participants.Any(participant => participant.IsBrazilian || participant.Members.Any(member => member.IsBrazilian));

    private Participant Resolve(Participant participant, Sport sport)
    {
        var members = participant.Members.Select(member => Resolve(member, sport)).ToList();
        var resolved = ApplySourceLayer(participant with { Members = members });

        if (resolved.Kind is ParticipantKind.Athlete)
        {
            resolved = WikidataIdentityRules.Apply(resolved, index.Find(resolved, sport));
        }

        return ApplyPairRule(resolved);
    }

    private static Participant ApplySourceLayer(Participant participant)
    {
        if (participant.DecidedBy is not null
            || !string.Equals(participant.Country, BrazilCountryCode, StringComparison.OrdinalIgnoreCase))
        {
            return participant;
        }

        return participant with
        {
            IsBrazilian = true,
            BrazilianReason = ReasonFor(participant.Kind),
            DecidedBy = IdentityLayer.Source,
        };
    }

    /// <summary>Dupla em que todos os atletas são brasileiros representa o Brasil.</summary>
    private static Participant ApplyPairRule(Participant participant)
    {
        if (participant.Kind != ParticipantKind.Pair
            || participant.IsBrazilian
            || participant.Members.Count == 0
            || !participant.Members.All(member => member.IsBrazilian))
        {
            return participant;
        }

        return participant with
        {
            IsBrazilian = true,
            BrazilianReason = Domain.BrazilianReason.RepresentsBrazil,
            DecidedBy = participant.Members[0].DecidedBy,
            IdentityConfidence = participant.Members.Max(member => member.IdentityConfidence),
        };
    }

    private static BrazilianReason ReasonFor(ParticipantKind kind) => kind switch
    {
        ParticipantKind.NationalTeam => Domain.BrazilianReason.BrazilianNationalTeam,
        ParticipantKind.Club => Domain.BrazilianReason.BrazilianClub,
        ParticipantKind.Organization => Domain.BrazilianReason.BrazilianOrganization,
        _ => Domain.BrazilianReason.RepresentsBrazil,
    };
}
