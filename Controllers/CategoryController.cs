using Black_order.DTOs.CategoryDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
using Black_order.Repositories.Categories;
using Black_order.Services.Files;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.ComponentModel;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]



    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IMapper mapper;
        private readonly IUserService userService;
        private readonly iFileStorage fileStorage;
        private const string cacheTag = "categories";
        private readonly string container = "categories";

        public CategoryController(ICategoryRepository categoryRepository,
            IOutputCacheStore outputCacheStore, IMapper mapper, IUserService userService, iFileStorage fileStorage)
        {
            this.categoryRepository = categoryRepository;
            this.outputCacheStore = outputCacheStore;
            this.mapper = mapper;
            this.userService = userService;
            this.fileStorage = fileStorage;
        }

        [HttpGet("GetCategories")]
        [OutputCache(Duration = 60, Tags = [cacheTag])]
        [AllowAnonymous]


        public async Task<ActionResult<List<CategoryDTO>>> Get()
        {
            //Getting categories from the database
            var categories = await categoryRepository.GetAllAsync();


            if (categories == null || categories.Count == 0)
            {
                return NotFound(new { message = "No categories found" });
            }

            
            var categoriesDTO = mapper.Map<List<CategoryDTO>>(categories);


            // Return the list of CategoryDTOs
            return Ok(categoriesDTO);
        }


        //GET CATEGORY BY ID
        //

        [HttpGet("{id}", Name = "GetCategoryById")]
        [OutputCache(Duration = 60, Tags = [cacheTag])]
        [AllowAnonymous]

        public async Task<ActionResult<CategoryDTO>> Get(string id)
        {

            var category = await categoryRepository.GetByIdAsync(id);


            if (category == null)
            {
                return NotFound(new { message = $"Category not found {id}" });
            }

            var categoryDTO = mapper.Map<CategoryDTO>(category);


            return Ok(categoryDTO);
        }


        [HttpPost("Add-Category")]
        public async Task<ActionResult<CategoryDTO>> Post([FromForm] CreateCategoryDTO category)
        {

   

            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            // Mapping CreateCategoryDTO to Category using Mapster
            var categoryEntity = mapper.Map<Category>(category);



            // Check if there are any images uploaded in the ProductCreateDTO
            if (category.Images != null && category.Images.Count > 0)
            {
                // Process the uploaded images
                foreach (var item in category.Images)
                {
                    var url = await fileStorage.Storage(container, item);

                    categoryEntity.Images.Add(
                        new CategoryImage
                        {
                            Url = url,
                            CategoryId = categoryEntity.Id
                        });
                }
            }


            //Adding the new category to the database
            await categoryRepository.AddAsync(categoryEntity);

            //Saving changes to the database
            await categoryRepository.SaveChangesAsync();



            // Evict the cache for categories to ensure the new category is reflected in subsequent requests
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            // Mapping Category to CategoryDTO using Mapster so we can return the newly created categoryDTO in the response
            var catDTO = mapper.Map<CategoryDTO>(categoryEntity);


            //Return 201 created and generate a location header with the URL of the newly created resource
            return CreatedAtAction(nameof(Get), new { id = categoryEntity.Id }, catDTO);
        }





        //EDIT CATEGORY

        [HttpPut("{id}")]

        public async Task<IActionResult> Put(string id, [FromForm] CreateCategoryDTO category)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Verifying if the category exists in the database
            var categoryExists = await categoryRepository.GetTrackedByIdAsync(id);

            // If the category does not exist, return a 404 Not Found response
            if (categoryExists == null)
            {
                return NotFound(new { message = "Category not found" });
            }

           


            //Removing existing images
            if (category.DeletedImages != null && category.DeletedImages.Count > 0)
            {
                foreach (var url in category.DeletedImages)
                {
                    var img = categoryExists.Images.FirstOrDefault(x => x.Url == url);

                    if (img != null)
                    {
                        categoryRepository.RemoveImage(img);
                    }
                }
            }



            if (category.Images != null && category.Images.Count > 0)
            {
                foreach (var file in category.Images)
                {
                    var url = await fileStorage.Storage(
                        container,
                        file);

                    categoryExists.Images.Add(new CategoryImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = url,
                        CategoryId = categoryExists.Id
                    });
                }
            }

           //Mapping from Catery to CategoryExist
            mapper.Map(category, categoryExists);


            // Save changes to the database
            await categoryRepository.SaveChangesAsync();


            // Evict the cache for categories to ensure the updated data is reflected
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            // Return a 204 No Content response to indicate successful update
            return NoContent();
        }

        //DELETE CATEGORY
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {

            // Verifying if the category exists in the database
            var category = await categoryRepository.GetTrackedByIdAsync(id);

            // If the category does not exist, return a 404 Not Found response
            if (category == null)
            {
                return NotFound(new {message = "Category not found"});
            }

            // Delete associated images from storage
            foreach (var img in category.Images)
            {
                // Assuming fileStorage.Delete is a method that deletes the file from storage
                await fileStorage.Delete(img.Url, container);
            }

            // Remove the category from the database
            categoryRepository.Remove(category);

            // Save changes to the database
            await categoryRepository.SaveChangesAsync();

            // Evict the cache for categories to ensure the deleted category is no longer reflected in subsequent requests
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            return NoContent();
        }
    }
}

