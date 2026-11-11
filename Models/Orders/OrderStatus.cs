namespace Black_order.Models.Orders
{
    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Processing = 2,
        Completed = 3,
        Cancelled = 4,
        Refunded = 5
    }
}
