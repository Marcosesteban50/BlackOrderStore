namespace Black_order.DTOs.OrderDTOs
{
    public class OrderSummaryDTO
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>Pending, Paid, Processing, Completed, Cancelled or Refunded.</summary>
        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public int ItemCount { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
