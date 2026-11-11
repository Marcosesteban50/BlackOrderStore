using Microsoft.AspNetCore.Identity;

namespace Black_order.Models.Guides
{
    public class Guide
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title { get; set; } = string.Empty;

        /// <summary>SEO friendly identifier used by the public route.</summary>
        public string Slug { get; set; } = string.Empty;

        public string? Summary { get; set; }

        /// <summary>Rich content authored by the admin (HTML).</summary>
        public string ContentHtml { get; set; } = string.Empty;

        public string? CoverUrl { get; set; }

        /// <summary>Free text such as "Beginner", "Intermediate", "Advanced".</summary>
        public string? Level { get; set; }

        public string? Expansion { get; set; }

        public bool IsPublished { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public int ViewCount { get; set; }

        public string? AuthorId { get; set; }

        public IdentityUser? Author { get; set; }

        public string? CategoryId { get; set; }

        public GuideCategory? Category { get; set; }

        public ICollection<GuideMedia> Media { get; set; } = new List<GuideMedia>();
    }
}
