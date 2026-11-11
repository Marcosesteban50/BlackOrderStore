using Black_order.DTOs.OrderDTOs;

namespace Black_order.DTOs.AdminBoardDTOs
{
    public class AdminDashboardDTO
    {
        public decimal TotalRevenue { get; set; }

        public int OrdersTotal { get; set; }

        public int OrdersToday { get; set; }

        public int PendingOrders { get; set; }

        public int PendingProducts { get; set; }

        public int Customers { get; set; }

        public int LowStockProducts { get; set; }

        public List<OrderSummaryDTO> RecentOrders { get; set; } = new List<OrderSummaryDTO>();

        public List<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();
    }

    public class TopProductDTO
    {
        public string? ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int UnitsSold { get; set; }

        public decimal Revenue { get; set; }
    }
}
