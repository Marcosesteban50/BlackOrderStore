using Black_order.Data;
using Black_order.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Users;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<List<UserDTO>> GetUsersWithClaimsAsync()
    {
        var usersWithClaims = await dbContext.Users.GroupJoin(
            dbContext.UserClaims,
            user => user.Id,
            claim => claim.UserId,
            (user, claims) => new { User = user, Claims = claims }
        ).ToListAsync();

        return usersWithClaims.Select(user => new UserDTO
        {
            Email = user.User.Email!,
            NickName = user.User.UserName!,
            Roles = user.Claims.Select(claim => $"{claim.ClaimType}: {claim.ClaimValue}").ToList()
        }).ToList();
    }
}
