using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Pipeline;

/// <summary>
/// Classifica o evento numa das duas visões do app (plano, seção 4.3).
/// Devolve <c>null</c> quando o evento não entra em nenhuma.
/// </summary>
public sealed class ViewClassifier
{
    public EventView? Classify(SportEvent sportEvent)
    {
        if (sportEvent.Scope == CompetitionScope.BrazilianDomestic)
        {
            return null;
        }

        return sportEvent.Sport.IsIndividual() ? ClassifyIndividualSport(sportEvent) : ClassifyTeamSport(sportEvent);
    }

    /// <summary>
    /// Atletas e duplas: quem representa o Brasil vai para o feed principal;
    /// quem só nasceu no Brasil (e representa outro país) vai para Indivíduos.
    /// </summary>
    private static EventView? ClassifyIndividualSport(SportEvent sportEvent)
    {
        var brazilians = sportEvent.Participants
            .SelectMany(participant => participant.Members.Prepend(participant))
            .Where(participant => participant.IsBrazilian)
            .ToList();

        if (brazilians.Count == 0)
        {
            return null;
        }

        return brazilians.Any(participant => participant.BrazilianReason != BrazilianReason.BornInBrazil)
            ? EventView.Main
            : EventView.Individuals;
    }

    /// <summary>
    /// Equipes: equipe brasileira em competição internacional vai para o feed principal;
    /// brasileiro em equipe estrangeira vai para Indivíduos.
    /// </summary>
    private static EventView? ClassifyTeamSport(SportEvent sportEvent)
    {
        var hasBrazilianTeam = sportEvent.Participants.Any(participant =>
            participant.IsBrazilian && participant.Kind is not ParticipantKind.Athlete);

        if (hasBrazilianTeam && sportEvent.Scope == CompetitionScope.International)
        {
            return EventView.Main;
        }

        var hasBrazilianMember = sportEvent.Participants
            .SelectMany(participant => participant.Members)
            .Any(member => member.IsBrazilian);

        return hasBrazilianMember ? EventView.Individuals : null;
    }
}
