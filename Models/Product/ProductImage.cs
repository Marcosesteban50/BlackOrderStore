namespace Black_order.Models
{
    public class ProductImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Url { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;

    }
}