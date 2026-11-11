namespace Black_order.DTOs.ReviewDTOs
{
    public class ReviewDTO
    {
        public string Id { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }

    public class ProductReviewsDTO
    {
        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public List<ReviewDTO> Items { get; set; } = new List<ReviewDTO>();
    }
}
