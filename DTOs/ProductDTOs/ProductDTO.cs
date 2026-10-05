using Black_order.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.ProductDTOs
{
    public class ProductDTO
    {
        public string? Id { get; set; }


        public string? Name { get; set; }

        public string? Description { get; set; }


        public decimal Price { get; set; }

        //public string? ImagenUrl { get; set; }

        //we are reading the List 
        public List<string> Images { get; set; } = new List<string>();
        public string? CategoryId { get; set; }

        public bool Available { get; set; }

        public string? CategoryName { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public bool Approved { get; set; } = false;
    }
}