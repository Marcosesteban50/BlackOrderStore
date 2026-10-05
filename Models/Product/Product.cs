using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Black_order.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        //public string? ImagenUrl { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public string? CategoryId { get; set; }

        public Category? Category { get; set; }

        public bool Available { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }

        public bool Approved { get; set; } = false;

    }
}