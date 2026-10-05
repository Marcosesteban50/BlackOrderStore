namespace Black_order.Models
{
    public class ProductHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}