namespace Black_order.DTOs.GuideDTOs
{
    public class GuideSummaryDTO
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string? CoverUrl { get; set; }

        public string? CategoryName { get; set; }

        public string? CategorySlug { get; set; }

        public string? Level { get; set; }

        public string? Expansion { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public bool IsPublished { get; set; }

        public int ViewCount { get; set; }
    }
}
