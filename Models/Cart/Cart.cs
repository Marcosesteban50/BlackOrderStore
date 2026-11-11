using Black_order.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Models.Cart
{
    public class Cart
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
 
}