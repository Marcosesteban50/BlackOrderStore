using Black_order.Models;
using Models.Cart;

namespace Black_order.Repositories.Carts;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task<Cart?> GetTrackedByUserIdAsync(string userId);
    Task<List<Product>> GetProductsByIdsAsync(IReadOnlyCollection<string> productIds);
    Task AddAsync(Cart cart);
    void RemoveItem(CartItem item);
    Task SaveChangesAsync();
}
