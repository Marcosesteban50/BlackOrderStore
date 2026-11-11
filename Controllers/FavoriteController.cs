using Black_order.DTOs.FavoriteDTOs;
using Black_order.Repositories.Favorites;
using Black_order.Repositories.Products;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Favorite;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public sealed class FavoriteController : ControllerBase
    {
        private readonly IFavoriteRepository favoriteRepository;
        private readonly IProductRepository productRepository;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public FavoriteController(
            IFavoriteRepository favoriteRepository,
            IProductRepository productRepository,
            IUserService userService,
            IMapper mapper)
        {
            this.favoriteRepository = favoriteRepository;
            this.productRepository = productRepository;
            this.userService = userService;
            this.mapper = mapper;
        }

        // GET: api/Favorite
        [HttpGet]
        public async Task<ActionResult<FavoriteDTO>> Get(CancellationToken cancellationToken)
        {
            var userId = await userService.RequireUserId();
            var favorite = await favoriteRepository.GetByUserIdAsync(userId);

            return Ok(favorite is null
                ? new FavoriteDTO()
                : mapper.Map<FavoriteDTO>(favorite));
        }

        // POST: api/Favorite/{productId}
        [HttpPost("{productId}")]
        public async Task<IActionResult> Add(string productId, CancellationToken cancellationToken)
        {
            var userId = await userService.RequireUserId();
            var product = await productRepository.GetByIdAsync(productId);

            if (product is null || !product.Approved)
            {
                return NotFound();
            }

            var favorite = await favoriteRepository.GetTrackedByUserIdAsync(userId);

            if (favorite is null)
            {
                favorite = new Favorite { UserId = userId };
                await favoriteRepository.AddAsync(favorite);
            }

            if (favorite.Items.Any(item => item.ProductId == productId))
            {
                return NoContent();
            }

            favorite.Items.Add(new FavoriteItems { ProductId = productId });
            await favoriteRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Favorite/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> Remove(string productId, CancellationToken cancellationToken)
        {
            var userId = await userService.RequireUserId();
            var favorite = await favoriteRepository.GetTrackedByUserIdAsync(userId);
            var item = favorite?.Items.FirstOrDefault(entry => entry.ProductId == productId);

            if (item is null)
            {
                return NoContent();
            }

            favoriteRepository.RemoveItem(item);
            await favoriteRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
