using Black_order.Data;
using Black_order.Models;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Categories;

public sealed class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync() => dbContext.Categories
        .AsNoTracking()
        .Include(category => category.Images)
        .ToListAsync();

    public Task<Category?> GetByIdAsync(string id)
    {
        return dbContext.Categories
            .AsNoTracking()
            .Include(category => category.Products)
            .Include(category => category.Images)
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public Task<Category?> GetTrackedByIdAsync(string id)
    {
        return dbContext.Categories
            .Include(category => category.Images)
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public async Task AddAsync(Category category) { await dbContext.AddAsync(category); }

    public void RemoveImage(CategoryImage image) { dbContext.CategoryImage.Remove(image); }

    public void Remove(Category category) { dbContext.Remove(category); }

    public Task SaveChangesAsync() { return dbContext.SaveChangesAsync(); }
}
