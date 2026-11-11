using Black_order.Data;
using Black_order.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Profiles;

public sealed class ProfileRepository(AppDbContext dbContext) : IProfileRepository
{
    public Task<UserProfile?> GetByUserIdAsync(string userId) => dbContext.UserProfiles
        .FirstOrDefaultAsync(profile => profile.UserId == userId);

    public Task AddAsync(UserProfile profile) => dbContext.UserProfiles.AddAsync(profile).AsTask();

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
