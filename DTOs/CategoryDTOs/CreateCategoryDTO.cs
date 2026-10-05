using Black_order.Models;
using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.CategoryDTOs
{
    public class CreateCategoryDTO
    {


        [MaxLength(25, ErrorMessage = "The name cannot exceed 25 characters")]
        public string? Name { get; set; }
        [MaxLength(500, ErrorMessage = "The description cannot exceed 500 characters")]

        public string Description { get; set; } = string.Empty;

        public List<IFormFile>? Images { get; set; }
        public List<string>? DeletedImages { get; set; }


    }

}