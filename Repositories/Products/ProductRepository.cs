using Black_order.Data;
using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Products;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public Task<List<Product>> GetAllAsync() => dbContext.Products
        .AsNoTracking()
        .Include(product => product.Category)
        .Include(product => product.Images)
        .ToListAsync();

    public Task<Product?> GetByIdAsync(string id) => dbContext.Products
        .AsNoTracking()
        .Include(product => product.Category)
        .Include(product => product.Images)
        .FirstOrDefaultAsync(product => product.Id == id);

    public Task<Product?> GetTrackedByIdAsync(string id) => dbContext.Products
        .FirstOrDefaultAsync(product => product.Id == id);

    public Task<Product?> GetByIdWithImagesAsync(string id) => dbContext.Products
        .Include(product => product.Images)
        .FirstOrDefaultAsync(product => product.Id == id);

    public async Task AddAsync(Product product) => await dbContext.AddAsync(product);

    public void Update(Product product) => dbContext.Update(product);

    public void Remove(Product product) => dbContext.Remove(product);

    public Task<string?> GetCategoryNameAsync(string? categoryId) => dbContext.Categories
        .Where(category => category.Id == categoryId)
        .Select(category => category.Name)
        .FirstOrDefaultAsync();

    public Task<List<Product>> GetFilteredAsync(ProductFilterDTO filter)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(product => product.Available);

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(product => product.Name!.Contains(filter.Name));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryId))
        {
            query = query.Where(product => product.CategoryId == filter.CategoryId);
        }

        if (filter.LowPrice)
        {
            query = query.OrderBy(product => product.Price);
        }
        else if (filter.HighPrice)
        {
            query = query.OrderByDescending(product => product.Price);
        }

        return query
            .Include(product => product.Category)
            .Include(product => product.Images)
            .ToListAsync();
    }

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
