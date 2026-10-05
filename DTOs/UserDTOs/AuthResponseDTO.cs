namespace Black_order.DTOs.UserDTOs
{
    public class AuthResponseDTO
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}