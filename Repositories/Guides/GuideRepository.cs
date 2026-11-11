using Black_order.Data;
using Black_order.DTOs.GuideDTOs;
using Black_order.Models.Guides;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Guides;

public sealed class GuideRepository(AppDbContext dbContext) : IGuideRepository
{
    public async Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetPublishedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Guides
            .AsNoTracking()
            .Where(guide => guide.IsPublished);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(guide =>
                guide.Title.Contains(search) ||
                (guide.Summary != null && guide.Summary.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.Trim();

            query = query.Where(guide => guide.Category != null && guide.Category.Slug == category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Level))
        {
            var level = filter.Level.Trim();

            query = query.Where(guide => guide.Level == level);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(guide => guide.Category)
            .OrderByDescending(guide => guide.PublishedAt ?? guide.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Guide?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guides
            .AsNoTracking()
            .Include(guide => guide.Category)
            .Include(guide => guide.Media.OrderBy(media => media.SortOrder))
            .FirstOrDefaultAsync(
                guide => guide.Slug == slug && guide.IsPublished,
                cancellationToken);
    }

    public async Task<IReadOnlyList<GuideCategory>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.GuideCategories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}
