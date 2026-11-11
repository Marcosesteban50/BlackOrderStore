namespace Black_order.Models.Guides
{
    /// <summary>
    /// A guide asset. Videos are hosted externally (YouTube/Vimeo) and referenced
    /// by URL; <see cref="ExternalId"/> is only set when the provider exposes an embeddable id.
    /// </summary>
    public class GuideMedia
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string GuideId { get; set; } = string.Empty;

        public Guide Guide { get; set; } = null!;

        public GuideMediaType MediaType { get; set; } = GuideMediaType.Video;

        /// <summary>"youtube", "vimeo" or "external".</summary>
        public string Provider { get; set; } = "youtube";

        public string Url { get; set; } = string.Empty;

        public string? ExternalId { get; set; }

        public string? Caption { get; set; }

        public int? DurationSeconds { get; set; }

        public int SortOrder { get; set; }
    }
}
