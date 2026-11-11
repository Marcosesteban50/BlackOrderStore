using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
using Black_order.Repositories.Products;
using Black_order.Services.Files;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public sealed class ProductController : ControllerBase
    {
        private const string CacheTag = "products";
        private const string Container = "products";

        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IProductRepository productRepository;
        private readonly iFileStorage fileStorage;
        private readonly IUserService userService;

        public ProductController(
            IMapper mapper,
            IOutputCacheStore outputCacheStore,
            IProductRepository productRepository,
            iFileStorage fileStorage,
            IUserService userService)
        {
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            this.productRepository = productRepository;
            this.fileStorage = fileStorage;
            this.userService = userService;
        }

        // GET: api/Product/GetProducts (solo aprobados)
        [HttpGet("GetProducts")]
        [AllowAnonymous]
        [OutputCache(Duration = 60, Tags = [CacheTag])]
        public async Task<ActionResult<List<ProductDTO>>> Get(CancellationToken cancellationToken)
        {
            var products = await productRepository.GetApprovedAsync();

            return Ok(mapper.Map<List<ProductDTO>>(products));
        }

        // GET: api/Product/{id}
        [HttpGet("{id}", Name = "GetProductById")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDTO>> Get(string id, CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            if (!product.Approved && !await CanManageAsync(product))
            {
                return NotFound();
            }

            return Ok(mapper.Map<ProductDTO>(product));
        }

        // GET: api/Product/Mine (productos del vendedor)
        [HttpGet("Mine")]
        [Authorize(Policy = "SellerOrAdmin")]
        public async Task<ActionResult<List<ProductDTO>>> Mine(CancellationToken cancellationToken)
        {
            var userId = await userService.RequireUserId();
            var products = await productRepository.GetBySellerAsync(userId);

            return Ok(mapper.Map<List<ProductDTO>>(products));
        }

        // POST: api/Product/Add-Product
        [HttpPost("Add-Product")]
        [Authorize(Policy = "SellerOrAdmin")]
        public async Task<ActionResult<ProductDTO>> Post(
            [FromForm] ProductCreateDTO productCreateDTO,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = await userService.RequireUserId();
            var product = mapper.Map<Product>(productCreateDTO);

            product.UserId = userId;
            product.Approved = false;
            product.Available = true;
            product.Stock = productCreateDTO.Stock;

            if (productCreateDTO.Images is not null)
            {
                foreach (var item in productCreateDTO.Images)
                {
                    var url = await fileStorage.Storage(Container, item);

                    product.Images.Add(new ProductImage
                    {
                        Url = url,
                        ProductId = product.Id
                    });
                }
            }

            await productRepository.AddAsync(product);
            await productRepository.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            var productDTO = mapper.Map<ProductDTO>(product);
            productDTO.CategoryName = await productRepository.GetCategoryNameAsync(product.CategoryId);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, productDTO);
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "SellerOrAdmin")]
        public async Task<ActionResult<ProductDTO>> Put(
            string id,
            [FromForm] ProductCreateDTO product,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var productExists = await productRepository.GetTrackedByIdAsync(id);

            if (productExists is null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(productExists))
            {
                return Forbid();
            }

            productExists.Name = product.Name;
            productExists.Description = product.Description;
            productExists.Price = product.Price;
            productExists.Stock = product.Stock;
            productExists.CategoryId = product.CategoryId;

            if (product.DeletedImages is not null)
            {
                foreach (var url in product.DeletedImages)
                {
                    var image = productExists.Images.FirstOrDefault(entry => entry.Url == url);

                    if (image is not null)
                    {
                        await fileStorage.Delete(image.Url, Container);
                        productExists.Images.Remove(image);
                    }
                }
            }

            if (product.Images is not null)
            {
                foreach (var file in product.Images)
                {
                    var url = await fileStorage.Storage(Container, file);

                    productExists.Images.Add(new ProductImage
                    {
                        Url = url,
                        ProductId = productExists.Id
                    });
                }
            }

            await productRepository.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            var productDTO = mapper.Map<ProductDTO>(productExists);
            productDTO.CategoryName = await productRepository.GetCategoryNameAsync(productExists.CategoryId);

            return Ok(productDTO);
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "SellerOrAdmin")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByIdWithImagesAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(product))
            {
                return Forbid();
            }

            foreach (var image in product.Images)
            {
                await fileStorage.Delete(image.Url, Container);
            }

            productRepository.Remove(product);
            await productRepository.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return NoContent();
        }

        // GET: api/Product/Filter
        [HttpGet("Filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDTO>>> Filter(
            [FromQuery] ProductFilterDTO productFilterDTO,
            CancellationToken cancellationToken)
        {
            var products = await productRepository.GetFilteredAsync(productFilterDTO);

            return Ok(mapper.Map<List<ProductDTO>>(products));
        }

        private async Task<bool> CanManageAsync(Product product)
        {
            if (User.HasClaim(claim => claim.Type == "Admin"))
            {
                return true;
            }

            var userId = await userService.GetUserId();

            return userId is not null && product.UserId == userId;
        }
    }
}
