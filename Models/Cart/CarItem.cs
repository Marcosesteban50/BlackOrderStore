using Black_order.Models;
using System.ComponentModel.DataAnnotations;

namespace Models.Cart
{
    public class CartItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProductId { get; set; } = null!;
        public Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        public string CartId { get; set; } = null!;
        public Cart Cart { get; set; } = null!;
    }
}