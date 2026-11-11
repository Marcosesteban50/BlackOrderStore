namespace Black_order.DTOs.GuideDTOs
{
    public class GuideDTO : GuideSummaryDTO
    {
        public string ContentHtml { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<GuideMediaDTO> Media { get; set; } = new List<GuideMediaDTO>();
    }
}
