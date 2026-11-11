using Black_order.Models.Reviews;

namespace Black_order.Repositories.Reviews;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByProductAsync(string productId, CancellationToken cancellationToken = default);

    Task<(double Average, int Count)> GetSummaryAsync(string productId, CancellationToken cancellationToken = default);

    Task<Review?> GetByUserAndProductAsync(
        string productId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<Review?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default);

    Task AddAsync(Review review, CancellationToken cancellationToken = default);

    void Remove(Review review);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
