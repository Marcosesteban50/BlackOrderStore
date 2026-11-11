using Black_order.Data;
using Black_order.DTOs.OrderDTOs;
using Black_order.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Orders;

public sealed class OrderRepository(AppDbContext dbContext) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => dbContext.Orders.AddAsync(order, cancellationToken).AsTask();

    public Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product!.Images)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<Order?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product!.Images)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByUserAsync(
        string userId,
        OrderFilterDTO filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId);

        query = ApplyFilters(query, filter);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllAsync(
        OrderFilterDTO filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking();

        query = ApplyFilters(query, filter);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderFilterDTO filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<OrderStatus>(filter.Status.Trim(), ignoreCase: true, out var status))
        {
            query = query.Where(order => order.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(order =>
                order.CustomerName.Contains(search) ||
                order.CustomerEmail.Contains(search) ||
                order.Id == search);
        }

        return query;
    }
}
