using Black_order.DTOs.GuideDTOs;
using Black_order.Models.Guides;

namespace Black_order.Repositories.Guides;

public interface IGuideRepository
{
    Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetPublishedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetManagedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default);

    Task<Guide?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Guide?> GetManagedByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<Guide?> GetTrackedByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuideCategory>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug,
        string? excludeId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Guide guide, CancellationToken cancellationToken = default);

    void Remove(Guide guide);

    Task<int> IncrementViewCountAsync(string slug, CancellationToken cancellationToken = default);

    Task<GuideCategory?> GetTrackedCategoryByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> CategorySlugExistsAsync(
        string slug,
        string? excludeId = null,
        CancellationToken cancellationToken = default);

    Task AddCategoryAsync(GuideCategory category, CancellationToken cancellationToken = default);

    void RemoveCategory(GuideCategory category);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
