using Black_order.Data;
using Black_order.Models.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Repositories.Reviews;

public sealed class ReviewRepository(AppDbContext dbContext) : IReviewRepository
{
    public async Task<IReadOnlyList<Review>> GetByProductAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.ProductId == productId && review.IsApproved)
            .Include(review => review.User)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(double Average, int Count)> GetSummaryAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.ProductId == productId && review.IsApproved);

        var count = await query.CountAsync(cancellationToken);

        if (count == 0)
        {
            return (0, 0);
        }

        var average = await query.AverageAsync(review => (double)review.Rating, cancellationToken);

        return (Math.Round(average, 2), count);
    }

    public Task<Review?> GetByUserAndProductAsync(
        string productId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reviews
            .FirstOrDefaultAsync(
                review => review.ProductId == productId && review.UserId == userId,
                cancellationToken);
    }

    public Task<Review?> GetTrackedByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reviews.FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
    }

    public Task AddAsync(Review review, CancellationToken cancellationToken = default)
        => dbContext.Reviews.AddAsync(review, cancellationToken).AsTask();

    public void Remove(Review review) => dbContext.Reviews.Remove(review);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
