using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Black_order.Services.UserServices
{

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        public UserService(IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<string?> GetUserId()
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // JWT emails can arrive as either mapping depending on token configuration.
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var usuario = await userManager.FindByEmailAsync(email);

            return usuario?.Id;
        }
    }
}
