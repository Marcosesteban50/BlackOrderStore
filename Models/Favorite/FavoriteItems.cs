using Black_order.Models;
using System.ComponentModel.DataAnnotations;

namespace Models.Favorite
{
    public class FavoriteItems
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProductId { get; set; } = null!;
        public Product Product { get; set; } = null!;

        [Required]
        public string FavoriteId { get; set; } = null!;
        public Favorite Favorite { get; set; } = null!;
    }
}
