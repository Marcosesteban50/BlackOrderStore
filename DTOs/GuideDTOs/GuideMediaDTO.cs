namespace Black_order.DTOs.GuideDTOs
{
    public class GuideMediaDTO
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>"Video" or "Image".</summary>
        public string MediaType { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? ExternalId { get; set; }

        public string? Caption { get; set; }

        public int? DurationSeconds { get; set; }

        public int SortOrder { get; set; }
    }
}
