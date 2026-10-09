using BrasilCompete.Worker.Identity.Wikidata;

namespace BrasilCompete.Worker.Identity;

public sealed record IdentityProfileSummary(
    string Key,
    IReadOnlyDictionary<BrazilianCriterion, int> CandidatesByCriterion,
    int Records,
    int Requests,
    IReadOnlyList<string> Warnings);
