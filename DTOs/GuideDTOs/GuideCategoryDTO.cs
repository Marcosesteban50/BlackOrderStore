namespace Black_order.DTOs.GuideDTOs
{
    public class GuideCategoryDTO
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }
}
