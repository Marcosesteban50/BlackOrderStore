using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.OrderDTOs
{
    public class CheckoutDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [MaxLength(256)]
        public string CustomerEmail { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CustomerDiscord { get; set; }

        [MaxLength(100)]
        public string? CharacterName { get; set; }

        [MaxLength(100)]
        public string? Realm { get; set; }

        [MaxLength(20)]
        public string? Region { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>"Manual" (transferencia) o "Stripe".</summary>
        public string PaymentMethod { get; set; } = "Manual";
    }
}
