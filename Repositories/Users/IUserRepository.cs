using Black_order.DTOs.UserDTOs;

namespace Black_order.Repositories.Users;

public interface IUserRepository
{
    Task<List<UserDTO>> GetUsersWithClaimsAsync();
}
