using Black_order.DTOs.ProductDTOs;


namespace Black_order.DTOs.CategoryDTOs
{
    public class CategoryDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }
        public List<string> Images { get; set; } = new List<string>();

        public List<ProductDTO>? Products { get; set; }

    }

}