using Black_order.Data;
using Black_order.Models;
using Microsoft.EntityFrameworkCore;
using Models.Cart;

namespace Black_order.Repositories.Carts;

public sealed class CartRepository(AppDbContext dbContext) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(string userId) => dbContext.Cart
        .AsNoTracking()
        .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
                .ThenInclude(product => product.Images)
        .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
                .ThenInclude(product => product.Category)
        .FirstOrDefaultAsync(cart => cart.UserId == userId);

    public Task<Cart?> GetTrackedByUserIdAsync(string userId) => dbContext.Cart
        .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
        .FirstOrDefaultAsync(cart => cart.UserId == userId);

    public Task<List<Product>> GetProductsByIdsAsync(IReadOnlyCollection<string> productIds) => dbContext.Products
        .AsNoTracking()
        .Where(product => productIds.Contains(product.Id))
        .ToListAsync();

    public Task AddAsync(Cart cart) => dbContext.Cart.AddAsync(cart).AsTask();

    public void RemoveItem(CartItem item) => dbContext.CartItems.Remove(item);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
