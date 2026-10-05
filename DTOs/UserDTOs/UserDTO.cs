namespace Black_order.DTOs.UserDTOs
{
    public class UserDTO
    {
        public required string Email { get; set; }
        public List<string>? Roles { get; set; }
    }
}