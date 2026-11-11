namespace Black_order.Models.Guides
{
    public class GuideCategory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public ICollection<Guide> Guides { get; set; } = new List<Guide>();
    }
}
