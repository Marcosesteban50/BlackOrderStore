using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.GoogleDTOs
{
    public class GoogleLoginRequestDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}