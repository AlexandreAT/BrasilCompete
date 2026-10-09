using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Tests.Normalization;

public sealed class TextNormalizerTests
{
    [Theory]
    [InlineData("São Paulo", "Sao Paulo")]
    [InlineData("Japão", "Japao")]
    [InlineData("Calderano", "Calderano")]
    public void RemoveDiacritics_RemovesAccents(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.RemoveDiacritics(input));

    [Theory]
    [InlineData("Gabriel Bortoleto", "gabriel-bortoleto")]
    [InlineData("  Alex 'Poatan' Pereira ", "alex-poatan-pereira")]
    [InlineData("GP de São Paulo — Corrida", "gp-de-sao-paulo-corrida")]
    public void Slugify_CreatesLowercaseAsciiSlug(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.Slugify(input));

    [Fact]
    public void NormalizeName_IgnoresCaseAccentsAndPunctuation() =>
        Assert.Equal(TextNormalizer.NormalizeName("João Fonseca"), TextNormalizer.NormalizeName("JOAO-FONSECA"));
}
