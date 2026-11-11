using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.CartDTOs;

public class AddCartItemsDTO
{
    [Required]
    public string ProductId { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}