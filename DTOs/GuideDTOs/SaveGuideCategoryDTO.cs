using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.GuideDTOs
{
    public class SaveGuideCategoryDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Optional manual slug. Generated from the name when empty.</summary>
        [MaxLength(120)]
        public string? Slug { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }
}
