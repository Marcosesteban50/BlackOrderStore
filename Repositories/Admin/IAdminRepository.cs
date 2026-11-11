using Black_order.DTOs.AdminBoardDTOs;
using Black_order.Models;

namespace Black_order.Repositories.Admin;

public interface IAdminRepository
{
    Task<AdminDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPendingProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Product?> GetTrackedProductAsync(string id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
