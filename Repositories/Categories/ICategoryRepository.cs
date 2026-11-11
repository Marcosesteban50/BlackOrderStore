using Black_order.Models;

namespace Black_order.Repositories.Categories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(string id);
    Task<Category?> GetTrackedByIdAsync(string id);
    Task AddAsync(Category category);
    void RemoveImage(CategoryImage image);
    void Remove(Category category);
    Task SaveChangesAsync();
}
