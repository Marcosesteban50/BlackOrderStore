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

        public async Task<string> GetUserId()
        {


            // Get the email claim from the current user's claims
            var email = httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "email")!.Value;


            // Check if the email is null or empty and throw an exception if it is
            if (string.IsNullOrEmpty(email))
            {
                throw new ApplicationException("Authentication required");


            }

            var usuario = await userManager.FindByEmailAsync(email);

            if (usuario == null)
            {
                throw new ApplicationException("Not Found");

            }

            return usuario!.Id;
        }




    }
}
