using Black_order.Data;
using Black_order.DTOs.AdminBoardDTOs;
using Black_order.DTOs.OrderDTOs;
using Black_order.Models;
using Black_order.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Admin;

public sealed class AdminRepository(AppDbContext dbContext) : IAdminRepository
{
    public async Task<AdminDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfDay = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        var paidOrders = dbContext.Orders
            .AsNoTracking()
            .Where(order => order.PaymentStatus == PaymentStatus.Paid);

        var totalRevenue = await paidOrders.SumAsync(order => (decimal?)order.Total, cancellationToken) ?? 0;

        var recentOrders = await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var topProducts = await dbContext.OrderItems
            .AsNoTracking()
            .Where(item => item.Order.PaymentStatus == PaymentStatus.Paid)
            .GroupBy(item => new { item.ProductId, item.ProductName })
            .Select(group => new TopProductDTO
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.ProductName,
                UnitsSold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.LineTotal)
            })
            .OrderByDescending(product => product.Revenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new AdminDashboardDTO
        {
            TotalRevenue = totalRevenue,
            OrdersTotal = await dbContext.Orders.CountAsync(cancellationToken),
            OrdersToday = await dbContext.Orders.CountAsync(
                order => order.CreatedAt >= startOfDay,
                cancellationToken),
            PendingOrders = await dbContext.Orders.CountAsync(
                order => order.Status == OrderStatus.Pending,
                cancellationToken),
            PendingProducts = await dbContext.Products.CountAsync(
                product => !product.Approved,
                cancellationToken),
            Customers = await dbContext.Users.CountAsync(cancellationToken),
            LowStockProducts = await dbContext.Products.CountAsync(
                product => product.Approved && product.Stock <= 5,
                cancellationToken),
            RecentOrders = recentOrders.Select(order => new OrderSummaryDTO
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                PaymentMethod = order.PaymentMethod,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Total = order.Total,
                ItemCount = order.Items.Count,
                CreatedAt = order.CreatedAt
            }).ToList(),
            TopProducts = topProducts
        };
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPendingProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(product => !product.Approved);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(product => product.Category)
            .Include(product => product.Images)
            .OrderBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Product?> GetTrackedProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
