using Black_order.Data;
using Microsoft.EntityFrameworkCore;
using Models.Favorite;

namespace Black_order.Repositories.Favorites;

public sealed class FavoriteRepository(AppDbContext dbContext) : IFavoriteRepository
{
    public Task<Favorite?> GetByUserIdAsync(string userId)
    {

        return dbContext.Favorites
        .AsNoTracking()
        .Include(favorite => favorite.Items)
            .ThenInclude(item => item.Product)
                .ThenInclude(product => product.Images)
        .FirstOrDefaultAsync(favorite => favorite.UserId == userId);
    }

    public async Task AddAsync(Favorite favorite)
    {
        await dbContext.Favorites.AddAsync(favorite);
    }

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
