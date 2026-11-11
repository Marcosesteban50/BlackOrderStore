using Black_order.DTOs.OrderDTOs;
using Black_order.Models.Orders;

namespace Black_order.Services.Orders;

public interface IOrderService
{
    Task<OrderDTO> CheckoutAsync(
        string userId,
        CheckoutDTO dto,
        CancellationToken cancellationToken = default);

    Task<OrderDTO> UpdateStatusAsync(
        string orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default);
}
