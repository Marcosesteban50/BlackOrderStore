using Black_order.DTOs.GuideDTOs;
using Black_order.Models.Guides;

namespace Black_order.Repositories.Guides;

public interface IGuideRepository
{
    Task<(IReadOnlyList<Guide> Items, int TotalCount)> GetPublishedPagedAsync(
        GuideFilterDTO filter,
        CancellationToken cancellationToken = default);

    Task<Guide?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuideCategory>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}
