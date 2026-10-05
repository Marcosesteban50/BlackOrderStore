using Black_order.Data;
using Black_order.DTOs.CategoryDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
using Black_order.Services.Files;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]

    public class ProductController : Controller
    {
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly AppDbContext dbContext;
        private readonly iFileStorage fileStorage;
        private readonly IUserService userService;
        private const string cacheTag = "products";
        private readonly string container = "products";

        public ProductController(IMapper mapper, IOutputCacheStore outputCacheStore,
            AppDbContext dbContext, iFileStorage fileStorage, IUserService userService)
        {
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            this.dbContext = dbContext;
            this.fileStorage = fileStorage;
            this.userService = userService;
        }



        [HttpGet("GetProducts")]
        [OutputCache(Duration = 60, Tags = [cacheTag])]
        [AllowAnonymous]

        public async Task<ActionResult<List<ProductDTO>>> Get()
        {

            // Getting all products from the database 
            var products = await dbContext.Products.AsNoTracking().Include(x => x.Category)
                .Include(x => x.Images).ToListAsync();


            //Mapping the products to ProductDTOs using Mapster


            // Check if the productsDTOs is null and return a NotFound response if it is
            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "No products found" });
            }

            var productsDTOs = mapper.Map<List<ProductDTO>>(products);
            // Return the list of ProductDTOs
            return productsDTOs;
        }

        //GET PRODUCT BY ID
        [HttpGet("{id}", Name = "GetProductById")]
        [OutputCache(Duration = 60, Tags = [cacheTag])]
        public async Task<ActionResult<ProductDTO>> Get(string id)
        {

            var product = await dbContext.Products.AsNoTracking().Include(x => x.Category)
                .Include(x => x.Images)
              .FirstOrDefaultAsync(x => x.Id == id);


            if (product == null)
            {
                return NotFound(new { message = $"Product not found {id}" });
            }

            var productDTO = mapper.Map<ProductDTO>(product);


            return productDTO;
        }




        //ADD PRODUCT 
        [HttpPost("Add-Product")]
        public async Task<ActionResult<ProductDTO>> Post([FromForm] ProductCreateDTO productCreateDTO)
        {

            var userId = await userService.GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { message = "User not authorized" });
            }


            // Check if the model state is valid, if not return a BadRequest response with the model state errors
            if (!ModelState.IsValid) { return BadRequest(ModelState); }


            // Map the ProductCreateDTO to Product using Mapster
            var products = mapper.Map<Product>(productCreateDTO);

            products.UserId = userId;
            products.Approved = false;
            products.Available = true;


            // Check if there are any images uploaded in the ProductCreateDTO
            if (productCreateDTO.Images != null)
            {
                // Process the uploaded images
                foreach (var item in productCreateDTO.Images)
                {
                    var url = await fileStorage.Storage(container, item);

                    products.Images.Add(
                        new ProductImage
                        {
                            Url = url,
                            ProductId = products.Id
                        });
                }
            }

            // Add the new product to the database
            dbContext.Add(products);

            // Save the changes to the database
            await dbContext.SaveChangesAsync();

            // Evict the cache for products to ensure the newly created product is reflected in subsequent requests
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            // Map the newly created product to ProductDTO using Mapster


            // Retrieve the category name for the newly created product and set it in the ProductDTO
            var CategoryName = await dbContext.Categories
                .Where(x => x.Id == products.CategoryId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            //Map from product to ProductDTO to return that to the user
            var productDTO = mapper.Map<ProductDTO>(products);
            //Set the categoryName to the new product
            productDTO.CategoryName = CategoryName;

            //Return 201 created and generate a location header with the URL of the newly created resource
            return CreatedAtAction(nameof(Get), new { id = productDTO.Id }, productDTO);


        }


        //EDIT PRODUCT BY ID

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> Put(string id, [FromForm] ProductCreateDTO product)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Verifying if the category exists in the database
            var productExists = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            // If the category does not exist, return a 404 Not Found response
            if (productExists != null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Mapping CreateProductDTO to Product using Mapster
            var productEntity = mapper.Map<Product>(product);

            // Set the ID of the product entity to the provided ID
            productEntity.Id = id;


            // Update the product in the database
            dbContext.Update(productEntity);

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            // Evict the cache for products to ensure the updated data is reflected
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            var productDTO = mapper.Map<ProductDTO>(productEntity);

            // Retrieve the category name for the updated product and set it in the ProductDTO
            productDTO.CategoryName = await dbContext.Categories
           .Where(x => x.Id == productDTO.CategoryId)
           .Select(x => x.Name)
           .FirstOrDefaultAsync();



            // Return the updated product DTO
            return Ok(productDTO);
        }


        //DELETE PRODUCT BY ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {

            // Check if the product exists in the database
            var product = await dbContext.Products
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            // if there are no products in the database return a NotFound response
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            foreach (var img in product.Images)
            {
                await fileStorage.Delete(img.Url, container);
            }

            dbContext.Remove(product);
            await dbContext.SaveChangesAsync();


            // Evict the cache for products to ensure the deleted product is not reflected in subsequent requests
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            return NoContent();
        }





        //-------------------------------------------------//



        [HttpGet("Filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDTO>>> Filter([FromQuery] ProductFilterDTO productFilterDTO)
        {
            // New query to get products from the database
            var queryProducts = dbContext.Products
                .AsNoTracking()
                .Where(x => x.Available)
                .AsQueryable();

            //Name Filter: If the name filter is not null or empty, filter the products by name
            if (!string.IsNullOrWhiteSpace(productFilterDTO.Name))
            {
                queryProducts = queryProducts
                    .Where(x => x.Name!.Contains(productFilterDTO.Name));
            }

            //Category Filter: If the category filter is not null or empty, filter the products by category
            if (!string.IsNullOrWhiteSpace(productFilterDTO.CategoryId))
            {
                queryProducts = queryProducts
                    .Where(x => x.CategoryId == productFilterDTO.CategoryId);
            }



            // Low to High Filter
            if (productFilterDTO.LowPrice)
            {
                queryProducts = queryProducts
                    .OrderBy(x => x.Price);
            }
            // High to Low Filter
            else if (productFilterDTO.HighPrice)
            {
                queryProducts = queryProducts
                    .OrderByDescending(x => x.Price);
            }

            // Execute the query and get the list of products from the database
            var products = await queryProducts.Include(x => x.Category)
                .Include(x => x.Images)
                .ToListAsync();



            // Check if the products list is null or empty and return a NotFound response if it is
            var productoDTO = mapper.Map<List<ProductDTO>>(products);



            return Ok(productoDTO);
        }


    }
}
