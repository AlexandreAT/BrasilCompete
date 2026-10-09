using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity;

/// <summary>
/// Camada do Wikidata (plano, seção 9.5): completa ou confirma o que a fonte informou.
/// </summary>
public static class WikidataIdentityRules
{
    public static Participant Apply(Participant participant, IdentityMatch? match)
    {
        if (match is null)
        {
            return participant;
        }

        var record = match.Record;
        var enriched = participant with { ExternalIds = WithWikidataId(participant.ExternalIds, record.WikidataId) };

        if (participant.DecidedBy == IdentityLayer.Manual || participant.IsBrazilian)
        {
            return enriched;
        }

        var sourceGaveAnotherCountry = participant.Country is not null;

        if (sourceGaveAnotherCountry)
        {
            // A fonte diz que o atleta representa outro país: só entra se nasceu no Brasil (visão Indivíduos).
            return record.BornInBrazil
                ? Brazilian(enriched, BrazilianReason.BornInBrazil, match.Confidence)
                : enriched with { BrazilianCitizenshipOnly = record.BrazilianCitizen };
        }

        if (record.RepresentsBrazil)
        {
            return Brazilian(enriched, BrazilianReason.RepresentsBrazil, match.Confidence);
        }

        if (record.BornInBrazil && record.RepresentsOtherCountries.Count > 0)
        {
            return Brazilian(enriched, BrazilianReason.BornInBrazil, match.Confidence);
        }

        if (record.BornInBrazil && record.BrazilianCitizen)
        {
            // Sem país esportivo registrado: nascido no Brasil e cidadão brasileiro é tratado como representante,
            // com confiança no máximo média (inferência, não fato registrado).
            return Brazilian(enriched, BrazilianReason.RepresentsBrazil, Max(match.Confidence, Confidence.Medium));
        }

        if (record.BornInBrazil)
        {
            return Brazilian(enriched, BrazilianReason.BornInBrazil, match.Confidence);
        }

        return enriched with { BrazilianCitizenshipOnly = record.IsCitizenshipOnly };
    }

    private static Participant Brazilian(Participant participant, BrazilianReason reason, Confidence confidence) => participant with
    {
        IsBrazilian = true,
        BrazilianReason = reason,
        DecidedBy = IdentityLayer.Wikidata,
        IdentityConfidence = confidence,
    };

    /// <summary>A pior das duas confianças (High &lt; Medium &lt; Low na ordem do enum).</summary>
    private static Confidence Max(Confidence first, Confidence second) => first > second ? first : second;

    private static IReadOnlyDictionary<string, string> WithWikidataId(IReadOnlyDictionary<string, string> ids, string wikidataId)
    {
        if (ids.ContainsKey(ExternalIdKeys.Wikidata))
        {
            return ids;
        }

        return new Dictionary<string, string>(ids) { [ExternalIdKeys.Wikidata] = wikidataId };
    }
}
