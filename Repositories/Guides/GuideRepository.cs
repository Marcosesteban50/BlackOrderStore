using Black_order.Data;
using Black_order.DTOs.GuideDTOs;
using Black_order.Models.Guides;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Guides;

public sealed class GuideRepository(AppDbContext dbContext) : IGuideRepository
{
    public Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetPublishedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default)
        => GetPagedAsync(filter, onlyPublished: true, cancellationToken);

    public Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetManagedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default)
        => GetPagedAsync(filter, onlyPublished: false, cancellationToken);

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

    public Task<Guide?> GetManagedByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guides
            .AsNoTracking()
            .Include(guide => guide.Category)
            .Include(guide => guide.Media.OrderBy(media => media.SortOrder))
            .FirstOrDefaultAsync(guide => guide.Id == id, cancellationToken);
    }

    public Task<Guide?> GetTrackedByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guides
            .Include(guide => guide.Category)
            .Include(guide => guide.Media)
            .FirstOrDefaultAsync(guide => guide.Id == id, cancellationToken);
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

    public Task<bool> SlugExistsAsync(
        string slug,
        string? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Guides.AnyAsync(
            guide => guide.Slug == slug && (excludeId == null || guide.Id != excludeId),
            cancellationToken);
    }

    public Task AddAsync(Guide guide, CancellationToken cancellationToken = default)
    {
        return dbContext.Guides.AddAsync(guide, cancellationToken).AsTask();
    }

    public void Remove(Guide guide) => dbContext.Guides.Remove(guide);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetPagedAsync(
        GuideFilterDTO filter,
        bool onlyPublished,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Guides.AsNoTracking();

        if (onlyPublished)
        {
            query = query.Where(guide => guide.IsPublished);
        }

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

        IQueryable<Guide> ordered = onlyPublished
            ? query.OrderByDescending(guide => guide.PublishedAt ?? guide.CreatedAt)
            : query.OrderByDescending(guide => guide.UpdatedAt);

        var items = await ordered
            .Include(guide => guide.Category)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
