using Black_order.DTOs.CartDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.Repositories.Carts;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Cart;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUserService userService;
        private readonly ICartRepository cartRepository;

        public CartController(IMapper mapper, IUserService userService, ICartRepository cartRepository)
        {
            this.mapper = mapper;
            this.userService = userService;
            this.cartRepository = cartRepository;
        }

        [HttpGet("GetCart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = await userService.RequireUserId();

            var cart = await cartRepository.GetByUserIdAsync(userId);

            if (cart is null)
            {
                return NotFound(new { message = "Cart not found." });
            }

            var items = cart.Items.Select(item => new
            {
                item.Id,
                item.ProductId,
                item.Quantity,
                Product = mapper.Map<ProductDTO>(item.Product)
            }).ToList();

            return Ok(new { cart.Id, Items = items });
        }


        [HttpPost("AddItem")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemsDTO dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await userService.RequireUserId();
            var products = await cartRepository.GetProductsByIdsAsync([dto.ProductId]);
            var product = products.FirstOrDefault();

            if (product is null)
            {
                return NotFound(new { message = "Product not found." });
            }

            if (!product.Available)
            {
                return BadRequest(new { message = "Product is not available." });
            }

            var cart = await cartRepository.GetTrackedByUserIdAsync(userId);
            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                await cartRepository.AddAsync(cart);
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == dto.ProductId);
            if (cartItem is null)
            {
                cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity });
            }
            else
            {
                if (dto.Quantity > int.MaxValue - cartItem.Quantity)
                {
                    return BadRequest(new { message = "The requested quantity is too large." });
                }

                cartItem.Quantity += dto.Quantity;
            }

            await cartRepository.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("RemoveItem/{productId}")]
        public async Task<IActionResult> RemoveItem(string productId)
        {
            var userId = await userService.RequireUserId();
            var cart = await cartRepository.GetTrackedByUserIdAsync(userId);
            if (cart is null)
            {
                return NotFound(new { message = "Cart not found." });
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem is null)
            {
                return NotFound(new { message = "Product is not in the cart." });
            }

            cartRepository.RemoveItem(cartItem);
            await cartRepository.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("RemoveOneItem/{id}")]
        public async Task<IActionResult> RemoveOneItem(string id)
        {
            var userId = await userService.RequireUserId();
            var cart = await cartRepository.GetTrackedByUserIdAsync(userId);
            if (cart is null)
            {
                return NotFound(new { message = "Cart not found." });
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == id);
            if (cartItem is null)
            {
                return NotFound(new { message = "Product is not in the cart." });
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity   --;
            }
            else
            {
                cartRepository.RemoveItem(cartItem);
            }

            await cartRepository.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = await userService.RequireUserId();
            var cart = await cartRepository.GetTrackedByUserIdAsync(userId);
            if (cart is null || cart.Items.Count == 0)
            {
                return NoContent();
            }

            foreach (var item in cart.Items)
            {
                cartRepository.RemoveItem(item);
            }

            await cartRepository.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("SyncCart")]
        public async Task<IActionResult> SyncCart([FromBody] List<SyncCartDTO> items)
        {
            if (items.Count == 0)
            {
                return NoContent();
            }

            var requestedAmounts = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                requestedAmounts.TryGetValue(item.ProductId, out var currentAmount);
                var combinedAmount = currentAmount + item.Quantity;
                if (combinedAmount > int.MaxValue)
                {
                    return BadRequest(new { message = "The requested amount is too large." });
                }

                requestedAmounts[item.ProductId] = combinedAmount;
            }

            var products = await cartRepository.GetProductsByIdsAsync(requestedAmounts.Keys.ToArray());
            if (products.Count != requestedAmounts.Count)
            {
                return NotFound(new { message = "One or more products were not found." });
            }

            if (products.Any(product => !product.Available))
            {
                return BadRequest(new { message = "One or more products are not available." });
            }

            var userId = await userService.RequireUserId();
            var cart = await cartRepository.GetTrackedByUserIdAsync(userId);
            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                await cartRepository.AddAsync(cart);
            }

            foreach (var (productId, amount) in requestedAmounts)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (amount + (cartItem?.Quantity ?? 0) > int.MaxValue)
                {
                    return BadRequest(new { message = "The requested amount is too large." });
                }
            }

            foreach (var (productId, amount) in requestedAmounts)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem is null)
                {
                    cart.Items.Add(new CartItem { ProductId = productId, Quantity = (int)amount });
                }
                else
                {
                    cartItem.Quantity += (int)amount;
                }
            }

            await cartRepository.SaveChangesAsync();
            return NoContent();
        }
    }

}
