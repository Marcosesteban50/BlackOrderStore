using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.ReviewDTOs
{
    public class CreateReviewDTO
    {
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string? Comment { get; set; }
    }
}
