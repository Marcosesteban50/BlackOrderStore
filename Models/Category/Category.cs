namespace Black_order.Models

{
    public class Category
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();


        public string? Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public ICollection<CategoryImage> Images { get; set; } = new List<CategoryImage>();

        public List<Product>? Products { get; set; }


    }
}
