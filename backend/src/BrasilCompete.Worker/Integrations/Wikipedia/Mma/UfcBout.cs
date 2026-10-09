namespace BrasilCompete.Worker.Integrations.Wikipedia.Mma;

/// <summary>Uma luta do card: categoria, os dois lutadores e o card em que está (principal, preliminar).</summary>
public sealed record UfcBout(string WeightClass, UfcFighter First, UfcFighter Second, string? Card);
