using Black_order.DTOs.OrderDTOs;
using Black_order.Models.Orders;

namespace Black_order.Repositories.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Order?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByUserAsync(
        string userId,
        OrderFilterDTO filter,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllAsync(
        OrderFilterDTO filter,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
