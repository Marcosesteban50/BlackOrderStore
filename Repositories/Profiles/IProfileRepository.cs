using Black_order.DTOs.UserDTOs;

namespace Black_order.Repositories.Profiles;

public interface IProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task AddAsync(UserProfile profile);
    Task SaveChangesAsync();
}
