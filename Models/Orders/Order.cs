using Microsoft.AspNetCore.Identity;

namespace Black_order.Models.Orders
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        /// <summary>"Manual" (transferencia) o "Stripe".</summary>
        public string PaymentMethod { get; set; } = "Manual";

        /// <summary>Identificador del pago externo (PaymentIntent, referencia manual, etc.).</summary>
        public string? PaymentReference { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerDiscord { get; set; }

        public string? CharacterName { get; set; }

        public string? Realm { get; set; }

        public string? Region { get; set; }

        public string? Notes { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? PaidAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
