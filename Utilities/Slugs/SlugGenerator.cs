using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Black_order.Utilities.Slugs;

public static partial class SlugGenerator
{
    private const int MaxLength = 180;

    /// <summary>Turns a title into an URL friendly slug (no accents, single dashes).</summary>
    public static string Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "guia";
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
            }
            else if (char.IsWhiteSpace(character) || character is '-' or '_' or '/' or '.')
            {
                builder.Append('-');
            }
        }

        var slug = Separators().Replace(builder.ToString(), "-").Trim('-');

        if (slug.Length == 0)
        {
            return "guia";
        }

        return slug.Length <= MaxLength ? slug : slug[..MaxLength].Trim('-');
    }

    [GeneratedRegex("-{2,}")]
    private static partial Regex Separators();
}
