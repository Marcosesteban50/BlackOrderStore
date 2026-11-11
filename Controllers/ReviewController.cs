using Black_order.DTOs.ReviewDTOs;
using Black_order.Models.Reviews;
using Black_order.Repositories.Products;
using Black_order.Repositories.Reviews;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public sealed class ReviewController : ControllerBase
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IProductRepository productRepository;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public ReviewController(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserService userService,
            IMapper mapper)
        {
            this.reviewRepository = reviewRepository;
            this.productRepository = productRepository;
            this.userService = userService;
            this.mapper = mapper;
        }

        // GET: api/Review/product/{productId}
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductReviewsDTO>> GetByProduct(
            string productId,
            CancellationToken cancellationToken)
        {
            var reviews = await reviewRepository.GetByProductAsync(productId, cancellationToken);
            var (average, count) = await reviewRepository.GetSummaryAsync(productId, cancellationToken);

            return Ok(new ProductReviewsDTO
            {
                AverageRating = average,
                ReviewCount = count,
                Items = mapper.Map<List<ReviewDTO>>(reviews)
            });
        }

        // POST: api/Review/product/{productId}
        [HttpPost("product/{productId}")]
        [Authorize]
        public async Task<ActionResult<ReviewDTO>> Create(
            string productId,
            [FromBody] CreateReviewDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var product = await productRepository.GetByIdAsync(productId);

            if (product is null || !product.Approved)
            {
                return NotFound();
            }

            var userId = await userService.RequireUserId();
            var existing = await reviewRepository.GetByUserAndProductAsync(
                productId,
                userId,
                cancellationToken);

            if (existing is not null)
            {
                existing.Rating = dto.Rating;
                existing.Comment = Normalize(dto.Comment);
                existing.CreatedAt = DateTimeOffset.UtcNow;
                existing.IsApproved = true;

                await reviewRepository.SaveChangesAsync(cancellationToken);

                return Ok(mapper.Map<ReviewDTO>(existing));
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = Normalize(dto.Comment),
                IsApproved = true
            };

            await reviewRepository.AddAsync(review, cancellationToken);
            await reviewRepository.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(
                nameof(GetByProduct),
                new { productId },
                mapper.Map<ReviewDTO>(review));
        }

        // DELETE: api/Review/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var review = await reviewRepository.GetTrackedByIdAsync(id, cancellationToken);

            if (review is null)
            {
                return NotFound();
            }

            var userId = await userService.RequireUserId();

            if (review.UserId != userId && !User.HasClaim(claim => claim.Type == "Admin"))
            {
                return Forbid();
            }

            reviewRepository.Remove(review);
            await reviewRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
