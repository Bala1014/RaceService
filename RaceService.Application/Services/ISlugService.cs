using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RaceService.Application.Services;

/// <summary>Generates URL-safe slugs.</summary>
public interface ISlugService
{
    /// <summary>Converts arbitrary text to a slug.</summary>
    string Generate(string value);

    /// <summary>
    /// Converts text to a slug and appends a numeric suffix until it no longer collides.
    /// </summary>
    /// <param name="value">Text to slugify.</param>
    /// <param name="exists">Predicate that reports whether a candidate is already taken.</param>
    Task<string> GenerateUniqueAsync(string value, Func<string, Task<bool>> exists);
}

/// <inheritdoc />
public sealed partial class SlugService : ISlugService
{
    /// <inheritdoc />
    public string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Decompose accented characters and drop the combining marks, so "Nürburgring" becomes
        // "nurburgring" rather than losing the vowel entirely.
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        slug = NonAlphanumeric().Replace(slug, "-");
        slug = MultipleHyphens().Replace(slug, "-").Trim('-');

        return slug.Length > 120 ? slug[..120].TrimEnd('-') : slug;
    }

    /// <inheritdoc />
    public async Task<string> GenerateUniqueAsync(string value, Func<string, Task<bool>> exists)
    {
        var baseSlug = Generate(value);

        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "item";
        }

        if (!await exists(baseSlug))
        {
            return baseSlug;
        }

        // Bounded rather than a while(true): if a hundred candidates are all taken something is
        // wrong upstream, and an unbounded loop would turn that into a hung request.
        for (var suffix = 2; suffix <= 100; suffix++)
        {
            var candidate = $"{baseSlug}-{suffix}";

            if (!await exists(candidate))
            {
                return candidate;
            }
        }

        return $"{baseSlug}-{Guid.CreateVersion7().ToString("n")[..8]}";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphens();
}
