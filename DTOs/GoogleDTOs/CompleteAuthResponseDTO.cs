using Black_order.DTOs.UserDTOs;

namespace Black_order.DTOs.GoogleDTOs
{
    public class CompleteAuthResponseDTO : AuthResponseDTO
    {
        public UserInfoDTO Usuario { get; set; } = new();
    }
}