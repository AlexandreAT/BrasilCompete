namespace BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

/// <summary>Um link interno <c>[[Alvo|Texto]]</c>. O alvo é o título do artigo.</summary>
public sealed record WikitextLink(string Target, string Text);
