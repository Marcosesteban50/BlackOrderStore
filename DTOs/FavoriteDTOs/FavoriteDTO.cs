namespace Black_order.DTOs.FavoriteDTOs
{
    public class FavoriteDTO
    {
        public string Id { get; set; } = null!;
        public List<FavoriteItemDTO> Items { get; set; } = new List<FavoriteItemDTO>();
    }
}