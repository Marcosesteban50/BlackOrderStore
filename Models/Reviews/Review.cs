using Microsoft.AspNetCore.Identity;

namespace Black_order.Models.Reviews
{
    public class Review
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ProductId { get; set; } = null!;

        public Product Product { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
