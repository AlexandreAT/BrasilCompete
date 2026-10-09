namespace BrasilCompete.Worker.Identity.Wikidata;

/// <summary>Os três caminhos pelos quais um atleta pode estar ligado ao Brasil no Wikidata.</summary>
public enum BrazilianCriterion
{
    /// <summary>País pelo qual compete (P1532) igual a Brasil.</summary>
    RepresentsBrazil,

    /// <summary>Local de nascimento (P19) num lugar do Brasil (P17).</summary>
    BornInBrazil,

    /// <summary>Cidadania brasileira (P27). Sozinha, não inclui o atleta (plano, seção 4.1).</summary>
    Citizenship,
}
