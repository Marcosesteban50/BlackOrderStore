using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.GuideDTOs
{
    public class CreateGuideDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        public string ContentHtml { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Level { get; set; }

        [MaxLength(50)]
        public string? Expansion { get; set; }

        public string? CategoryId { get; set; }

        /// <summary>Optional manual slug. Generated from the title when empty.</summary>
        public string? Slug { get; set; }

        public bool IsPublished { get; set; }

        public IFormFile? Cover { get; set; }

        public bool RemoveCover { get; set; }

        public string? VideoUrl { get; set; }

        [MaxLength(300)]
        public string? VideoCaption { get; set; }

        public bool RemoveVideo { get; set; }
    }
}
