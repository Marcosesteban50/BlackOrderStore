namespace Black_order.Models
{
    public class CategoryImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Url { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;

        public Category Category { get; set; } = null!;
    }
}
