
namespace Black_order.DTOs.ProductDTOs
{

public class ProductFilterDTO
    {

        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public bool Available { get; set; }
        public bool HighPrice { get; set; }
        public bool LowPrice { get; set; }
   
    }
}