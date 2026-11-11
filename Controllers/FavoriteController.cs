using Black_order.Data;
using Black_order.DTOs.FavoriteDTOs;
using Black_order.Repositories.Favorites;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly AppDbContext dbContext;
        private readonly IUserService userService;
        private readonly IFavoriteRepository favoriteRepository;

        public FavoriteController(IMapper mapper,AppDbContext dbContext,IUserService userService, IFavoriteRepository favoriteRepository)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            this.userService = userService;
            this.favoriteRepository = favoriteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = await userService.RequireUserId();
            var favorite = await favoriteRepository.GetByUserIdAsync(userId);

            if (favorite is null)
            {
                return NotFound(new { message = "Favorites not found." });
            }
       

          var dto = mapper.Map<FavoriteDTO>(favorite);

            return Ok(dto);
        }
    }
    
}
