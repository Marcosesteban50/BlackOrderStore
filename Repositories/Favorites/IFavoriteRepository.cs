using Models.Favorite;

namespace Black_order.Repositories.Favorites;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByUserIdAsync(string userId);

    Task<Favorite?> GetTrackedByUserIdAsync(string userId);

    Task AddAsync(Favorite favorite);

    void RemoveItem(FavoriteItems item);

    Task SaveChangesAsync();
}
