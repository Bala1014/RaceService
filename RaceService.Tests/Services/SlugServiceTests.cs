using FluentAssertions;
using RaceService.Application.Services;

namespace RaceService.Tests.Services;

/// <summary>Slugs are the public URL key, so their edge cases are worth pinning down.</summary>
public class SlugServiceTests
{
    private readonly SlugService _slugs = new();

    [Theory]
    [InlineData("Monaco Grand Prix", "monaco-grand-prix")]
    [InlineData("24 Hours of Le Mans", "24-hours-of-le-mans")]
    [InlineData("  Spa-Francorchamps  ", "spa-francorchamps")]
    [InlineData("Formula 1 / FIA", "formula-1-fia")]
    [InlineData("Autódromo José Carlos Pace", "autodromo-jose-carlos-pace")]
    [InlineData("Nürburgring", "nurburgring")]
    [InlineData("Circuit  de   Monaco", "circuit-de-monaco")]
    [InlineData("---odd---", "odd")]
    [InlineData("", "")]
    public void Generates_url_safe_slugs(string input, string expected)
        => _slugs.Generate(input).Should().Be(expected);

    [Fact]
    public async Task Appends_a_suffix_until_the_slug_is_free()
    {
        var taken = new HashSet<string> { "monaco-grand-prix", "monaco-grand-prix-2" };

        var slug = await _slugs.GenerateUniqueAsync(
            "Monaco Grand Prix",
            candidate => Task.FromResult(taken.Contains(candidate)));

        slug.Should().Be("monaco-grand-prix-3");
    }

    [Fact]
    public async Task Falls_back_to_a_placeholder_when_the_text_has_no_usable_characters()
    {
        var slug = await _slugs.GenerateUniqueAsync("!!!", _ => Task.FromResult(false));

        slug.Should().Be("item");
    }

    [Fact]
    public void Keeps_slugs_within_the_column_length()
        => _slugs.Generate(new string('a', 400)).Length.Should().BeLessThanOrEqualTo(120);
}
