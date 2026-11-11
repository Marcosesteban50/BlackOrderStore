namespace Black_order.Models.Orders
{
    /// <summary>
    /// Snapshot of a purchased product. Price and name are copied so the order
    /// history never changes if the catalog changes or the product is deleted.
    /// </summary>
    public class OrderItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string OrderId { get; set; } = null!;

        public Order Order { get; set; } = null!;

        public string? ProductId { get; set; }

        public Product? Product { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}
