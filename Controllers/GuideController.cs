using Black_order.DTOs.Common;
using Black_order.DTOs.GuideDTOs;
using Black_order.Repositories.Guides;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public sealed class GuideController : ControllerBase
    {
        private const string CacheTag = "guides";

        private readonly IGuideRepository guideRepository;
        private readonly IMapper mapper;

        public GuideController(IGuideRepository guideRepository, IMapper mapper)
        {
            this.guideRepository = guideRepository;
            this.mapper = mapper;
        }

        // GET: api/Guide?page=1&pageSize=12&search=&category=&level=
        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Duration = 120, Tags = [CacheTag])]
        public async Task<ActionResult<PagedResult<GuideSummaryDTO>>> GetGuides(
            [FromQuery] GuideFilterDTO filter,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await guideRepository
                .GetPublishedPagedAsync(filter, cancellationToken);

            var result = new PagedResult<GuideSummaryDTO>
            {
                Items = mapper.Map<List<GuideSummaryDTO>>(items),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // GET: api/Guide/categories
        [HttpGet("categories")]
        [AllowAnonymous]
        [OutputCache(Duration = 300, Tags = [CacheTag])]
        public async Task<ActionResult<List<GuideCategoryDTO>>> GetCategories(
            CancellationToken cancellationToken)
        {
            var categories = await guideRepository.GetCategoriesAsync(cancellationToken);

            return Ok(mapper.Map<List<GuideCategoryDTO>>(categories));
        }

        // GET: api/Guide/guia-rapida-1-80
        [HttpGet("{slug}", Name = "GetGuideBySlug")]
        [AllowAnonymous]
        [OutputCache(Duration = 120, Tags = [CacheTag])]
        public async Task<ActionResult<GuideDTO>> GetGuide(
            string slug,
            CancellationToken cancellationToken)
        {
            var guide = await guideRepository.GetPublishedBySlugAsync(slug, cancellationToken);

            if (guide is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Guide not found",
                    detail: $"No published guide exists with slug '{slug}'.");
            }

            return Ok(mapper.Map<GuideDTO>(guide));
        }
    }
}
