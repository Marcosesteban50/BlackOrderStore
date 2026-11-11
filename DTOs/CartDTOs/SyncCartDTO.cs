using System.ComponentModel.DataAnnotations;

namespace Black_order.DTOs.CartDTOs;

public class SyncCartDTO
{
    [Required]
    public string ProductId { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }
}