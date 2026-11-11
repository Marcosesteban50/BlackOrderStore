namespace Black_order.DTOs.OrderDTOs
{
    public class OrderDTO : OrderSummaryDTO
    {
        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerDiscord { get; set; }

        public string? CharacterName { get; set; }

        public string? Realm { get; set; }

        public string? Region { get; set; }

        public string? Notes { get; set; }

        public string? PaymentReference { get; set; }

        public DateTimeOffset? PaidAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }

        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
