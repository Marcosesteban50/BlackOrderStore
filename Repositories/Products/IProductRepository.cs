using Black_order.DTOs.ProductDTOs;
using Black_order.Models;

namespace Black_order.Repositories.Products;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetApprovedAsync();
    Task<List<Product>> GetBySellerAsync(string sellerId);
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetTrackedByIdAsync(string id);
    Task<Product?> GetByIdWithImagesAsync(string id);
    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task<string?> GetCategoryNameAsync(string? categoryId);
    Task<List<Product>> GetFilteredAsync(ProductFilterDTO filter);
    Task SaveChangesAsync();
}
