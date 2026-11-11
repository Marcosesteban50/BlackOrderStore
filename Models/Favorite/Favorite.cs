using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Models.Favorite
{
    public class Favorite
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
        public List<FavoriteItems> Items { get; set; } = new();
    }
}
