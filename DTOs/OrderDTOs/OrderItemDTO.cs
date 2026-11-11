namespace Black_order.DTOs.OrderDTOs
{
    public class OrderItemDTO
    {
        public string Id { get; set; } = string.Empty;

        public string? ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }

        public string? ImageUrl { get; set; }
    }
}
