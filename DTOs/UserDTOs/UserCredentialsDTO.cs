using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.UserDTOs
{

    public class UserCredentialsDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }

    }
}
