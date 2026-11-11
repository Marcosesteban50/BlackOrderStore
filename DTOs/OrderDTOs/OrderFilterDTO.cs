using Black_order.Models.Orders;

namespace Black_order.DTOs.OrderDTOs
{
    public class OrderFilterDTO
    {
        private int page = 1;
        private int pageSize = 10;

        public int Page
        {
            get => page;
            set => page = Math.Max(1, value);
        }

        public int PageSize
        {
            get => pageSize;
            set => pageSize = Math.Clamp(value, 1, 50);
        }

        /// <summary>Optional status filter: Pending, Paid, Processing, Completed, Cancelled, Refunded.</summary>
        public string? Status { get; set; }

        /// <summary>Searches by customer name or email.</summary>
        public string? Search { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        public OrderStatus Status { get; set; }

        /// <summary>Optional internal note about the change.</summary>
        public string? Note { get; set; }
    }
}
