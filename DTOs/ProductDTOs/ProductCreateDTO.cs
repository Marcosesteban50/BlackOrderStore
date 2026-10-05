using Black_order.Models;
using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.ProductDTOs
{
    public class ProductCreateDTO
    {

        [Required(ErrorMessage = "The name is required")]
        [MaxLength(25, ErrorMessage = "The name cannot exceed 25 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "The description is required")]
        [MaxLength(100, ErrorMessage = "The description cannot exceed 100 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "The price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "The price must be a positive number")]
        public decimal Price { get; set; }

        //public string? ImagenUrl { get; set; }
        public List<IFormFile>? Images { get; set; }
        [Required(ErrorMessage = "The category is required")]
        public string CategoryId { get; set; } = string.Empty;

        public List<string>? DeletedImages { get; set; }



    }
}