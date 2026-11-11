namespace Black_order.DTOs.FavoriteDTOs
{
    public class FavoriteItemDTO
    {
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }

        public decimal Price { get; set; }

        public List<string> Images { get; set; } = new();
    }
}