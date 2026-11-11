using Black_order.DTOs.UserDTOs;
using Black_order.Repositories.Profiles;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
  
    [ApiController]
    [Route("api/[Controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository profileRepository;
        private readonly IUserService userService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;

        public ProfileController(IProfileRepository profileRepository,IUserService userService,
            UserManager<IdentityUser> userManager,IMapper mapper)
        {
            this.profileRepository = profileRepository;
            this.userService = userService;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        [HttpGet("Get-Profile")]
        public async Task<ActionResult<UserProfileDTO>> GetProfile()
        {
            var userId = await userService.GetUserId();

            if(userId == null)
            {
                return Unauthorized();
            }

            var email = await profileRepository.GetByUserIdAsync(userId);

            var userProfile = await profileRepository.GetByUserIdAsync(userId);

            if(userProfile == null)
            {
                return NotFound();
            }


            var mapped = mapper.Map<UserProfileDTO>(userProfile);  
            
            mapped.Email = email!.Email!;


            return Ok(mapped);
        }



        [HttpPut("Update-Profile")]
        public async Task<ActionResult<UserProfileDTO>> UpdateProfile([FromBody] UserProfileDTO userProfileDto)
        {
            var userId = await userService.GetUserId();

            if(userId == null)
            {
                return Unauthorized();
            }

            var userProfile = await profileRepository.GetByUserIdAsync(userId);

            if(userProfile == null)
            {
                return NotFound();
            }

            userProfile.FullName = string.IsNullOrWhiteSpace(userProfileDto.Name)
                ? userProfile.FullName
                : userProfileDto.Name.Trim();

            userProfile.Address = string.IsNullOrWhiteSpace(userProfileDto.Adress)
                ? userProfile.Address
                : userProfileDto.Adress.Trim();

            userProfile.Phone = string.IsNullOrWhiteSpace(userProfileDto.Phone)
                ? userProfile.Phone
                : userProfileDto.Phone.Trim();

            userProfile.NickName = string.IsNullOrWhiteSpace(userProfileDto.NickName)
                ? userProfile.NickName
                : userProfileDto.NickName.Trim();

            await profileRepository.SaveChangesAsync();

            var mapped = mapper.Map<UserProfileDTO>(userProfile);
            mapped.Email = userProfile.Email!;

            return Ok(mapped);
        }

    }
}
