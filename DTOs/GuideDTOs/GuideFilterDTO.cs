namespace Black_order.DTOs.GuideDTOs
{
    public class GuideFilterDTO
    {
        private const int MaxPageSize = 50;

        private int page = 1;
        private int pageSize = 12;

        public int Page
        {
            get => page;
            set => page = Math.Max(1, value);
        }

        public int PageSize
        {
            get => pageSize;
            set => pageSize = Math.Clamp(value, 1, MaxPageSize);
        }

        public string? Search { get; set; }

        /// <summary>Category slug.</summary>
        public string? Category { get; set; }

        public string? Level { get; set; }
    }
}
