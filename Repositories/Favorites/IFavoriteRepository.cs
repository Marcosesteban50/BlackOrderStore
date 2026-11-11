using Models.Favorite;

namespace Black_order.Repositories.Favorites;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByUserIdAsync(string userId);
}
