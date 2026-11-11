namespace Black_order.Utilities.Guides;

public static class VideoUrlParser
{
    /// <summary>
    /// Detects the video provider and the embeddable id from a watch URL.
    /// Unknown hosts degrade to provider "external" (the UI shows a link).
    /// </summary>
    public static (string Provider, string? ExternalId) Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
        {
            return ("external", null);
        }

        var host = uri.Host.ToLowerInvariant();

        if (host is "youtu.be" or "www.youtu.be")
        {
            var shortId = uri.AbsolutePath.Trim('/');
            return ("youtube", string.IsNullOrWhiteSpace(shortId) ? null : shortId);
        }

        if (host.EndsWith("youtube.com", StringComparison.Ordinal))
        {
            var id = ReadQuery(uri.Query, "v");

            if (string.IsNullOrWhiteSpace(id) &&
                uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
            {
                id = uri.AbsolutePath["/embed/".Length..].Trim('/');
            }

            return ("youtube", string.IsNullOrWhiteSpace(id) ? null : id);
        }

        if (host.EndsWith("vimeo.com", StringComparison.Ordinal))
        {
            var id = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return ("vimeo", id);
        }

        return ("external", null);
    }

    private static string? ReadQuery(string query, string key)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);

            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
