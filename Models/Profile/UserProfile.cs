using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.UserDTOs
{
    public class UserProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }

        public string? Email { get; set; }


        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }
    }
}