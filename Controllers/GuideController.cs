using Black_order.DTOs.Common;
using Black_order.DTOs.GuideDTOs;
using Black_order.Models.Guides;
using Black_order.Repositories.Guides;
using Black_order.Services.Files;
using Black_order.Services.UserServices;
using Black_order.Utilities.Guides;
using Black_order.Utilities.Slugs;
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
        private const string CoverContainer = "guides";
        private const long MaxCoverBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedCoverExtensions =
            [".jpg", ".jpeg", ".png", ".webp"];

        private readonly IGuideRepository guideRepository;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IUserService userService;
        private readonly iFileStorage fileStorage;

        public GuideController(
            IGuideRepository guideRepository,
            IMapper mapper,
            IOutputCacheStore outputCacheStore,
            IUserService userService,
            iFileStorage fileStorage)
        {
            this.guideRepository = guideRepository;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            this.userService = userService;
            this.fileStorage = fileStorage;
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

        // POST: api/Guide/categories
        [HttpPost("categories")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<GuideCategoryDTO>> CreateCategory(
            [FromBody] SaveGuideCategoryDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = new GuideCategory
            {
                Name = dto.Name.Trim(),
                Slug = await BuildUniqueCategorySlugAsync(dto.Slug, dto.Name, null, cancellationToken),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                SortOrder = dto.SortOrder
            };

            await guideRepository.AddCategoryAsync(category, cancellationToken);
            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return CreatedAtAction(
                nameof(GetCategories),
                new { },
                mapper.Map<GuideCategoryDTO>(category));
        }

        // PUT: api/Guide/categories/{id}
        [HttpPut("categories/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<GuideCategoryDTO>> UpdateCategory(
            string id,
            [FromBody] SaveGuideCategoryDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = await guideRepository.GetTrackedCategoryByIdAsync(id, cancellationToken);

            if (category is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Category not found",
                    detail: $"No guide category exists with id '{id}'.");
            }

            category.Name = dto.Name.Trim();
            category.Slug = await BuildUniqueCategorySlugAsync(dto.Slug, dto.Name, category.Id, cancellationToken);
            category.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            category.SortOrder = dto.SortOrder;

            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return Ok(mapper.Map<GuideCategoryDTO>(category));
        }

        // DELETE: api/Guide/categories/{id}
        [HttpDelete("categories/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteCategory(string id, CancellationToken cancellationToken)
        {
            var category = await guideRepository.GetTrackedCategoryByIdAsync(id, cancellationToken);

            if (category is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Category not found",
                    detail: $"No guide category exists with id '{id}'.");
            }

            guideRepository.RemoveCategory(category);
            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return NoContent();
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

        // POST: api/Guide/{slug}/view
        [HttpPost("{slug}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterView(string slug, CancellationToken cancellationToken)
        {
            // Best effort: the public page calls this once per visit and the cache is not evicted.
            await guideRepository.IncrementViewCountAsync(slug, cancellationToken);

            return NoContent();
        }

        // GET: api/Guide/manage?page=1&pageSize=10&search=
        [HttpGet("manage")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<PagedResult<GuideSummaryDTO>>> GetManaged(
            [FromQuery] GuideFilterDTO filter,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await guideRepository
                .GetManagedPagedAsync(filter, cancellationToken);

            return Ok(new PagedResult<GuideSummaryDTO>
            {
                Items = mapper.Map<List<GuideSummaryDTO>>(items),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            });
        }

        // GET: api/Guide/manage/{id}
        [HttpGet("manage/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<GuideDTO>> GetManagedById(
            string id,
            CancellationToken cancellationToken)
        {
            var guide = await guideRepository.GetManagedByIdAsync(id, cancellationToken);

            if (guide is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Guide not found",
                    detail: $"No guide exists with id '{id}'.");
            }

            return Ok(mapper.Map<GuideDTO>(guide));
        }

        // POST: api/Guide
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<GuideDTO>> Create(
            [FromForm] CreateGuideDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (ValidateCover(dto.Cover) is { } coverError)
            {
                return coverError;
            }

            if (!IsValidVideoUrl(dto.VideoUrl))
            {
                ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL is not valid.");
                return ValidationProblem(ModelState);
            }

            var guide = mapper.Map<Guide>(dto);
            var now = DateTimeOffset.UtcNow;

            guide.AuthorId = await userService.RequireUserId();
            guide.Slug = await BuildUniqueSlugAsync(dto.Slug, dto.Title, null, cancellationToken);
            guide.CreatedAt = now;
            guide.UpdatedAt = now;
            guide.PublishedAt = dto.IsPublished ? now : null;

            if (dto.Cover is not null)
            {
                guide.CoverUrl = await fileStorage.Storage(CoverContainer, dto.Cover);
            }

            AddVideo(guide, dto);

            await guideRepository.AddAsync(guide, cancellationToken);
            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            var created = await guideRepository.GetManagedByIdAsync(guide.Id, cancellationToken) ?? guide;

            return CreatedAtAction(
                nameof(GetGuide),
                new { slug = guide.Slug },
                mapper.Map<GuideDTO>(created));
        }

        // PUT: api/Guide/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<GuideDTO>> Update(
            string id,
            [FromForm] CreateGuideDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (ValidateCover(dto.Cover) is { } coverError)
            {
                return coverError;
            }

            if (!IsValidVideoUrl(dto.VideoUrl))
            {
                ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL is not valid.");
                return ValidationProblem(ModelState);
            }

            var guide = await guideRepository.GetTrackedByIdAsync(id, cancellationToken);

            if (guide is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Guide not found",
                    detail: $"No guide exists with id '{id}'.");
            }

            mapper.Map(dto, guide);

            guide.Slug = await BuildUniqueSlugAsync(dto.Slug, dto.Title, guide.Id, cancellationToken);
            guide.UpdatedAt = DateTimeOffset.UtcNow;

            if (dto.IsPublished)
            {
                guide.PublishedAt ??= guide.UpdatedAt;
            }
            else
            {
                guide.PublishedAt = null;
            }

            if (dto.Cover is not null)
            {
                if (!string.IsNullOrWhiteSpace(guide.CoverUrl))
                {
                    await fileStorage.Delete(guide.CoverUrl, CoverContainer);
                }

                guide.CoverUrl = await fileStorage.Storage(CoverContainer, dto.Cover);
            }
            else if (dto.RemoveCover && !string.IsNullOrWhiteSpace(guide.CoverUrl))
            {
                await fileStorage.Delete(guide.CoverUrl, CoverContainer);
                guide.CoverUrl = null;
            }

            if (dto.RemoveVideo)
            {
                foreach (var video in guide.Media.Where(media => media.MediaType == GuideMediaType.Video).ToList())
                {
                    guide.Media.Remove(video);
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.VideoUrl))
            {
                AddVideo(guide, dto);
            }

            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return Ok(mapper.Map<GuideDTO>(guide));
        }

        // DELETE: api/Guide/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var guide = await guideRepository.GetTrackedByIdAsync(id, cancellationToken);

            if (guide is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Guide not found",
                    detail: $"No guide exists with id '{id}'.");
            }

            if (!string.IsNullOrWhiteSpace(guide.CoverUrl))
            {
                await fileStorage.Delete(guide.CoverUrl, CoverContainer);
            }

            guideRepository.Remove(guide);
            await guideRepository.SaveChangesAsync(cancellationToken);
            await outputCacheStore.EvictByTagAsync(CacheTag, cancellationToken);

            return NoContent();
        }

        private static void AddVideo(Guide guide, CreateGuideDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.VideoUrl))
            {
                return;
            }

            var (provider, externalId) = VideoUrlParser.Parse(dto.VideoUrl);
            var existing = guide.Media.FirstOrDefault(media => media.MediaType == GuideMediaType.Video);

            if (existing is null)
            {
                guide.Media.Add(new GuideMedia
                {
                    Provider = provider,
                    ExternalId = externalId,
                    Url = dto.VideoUrl.Trim(),
                    Caption = string.IsNullOrWhiteSpace(dto.VideoCaption) ? null : dto.VideoCaption.Trim(),
                    SortOrder = 0
                });

                return;
            }

            existing.Provider = provider;
            existing.ExternalId = externalId;
            existing.Url = dto.VideoUrl.Trim();
            existing.Caption = string.IsNullOrWhiteSpace(dto.VideoCaption) ? null : dto.VideoCaption.Trim();
        }

        private static bool IsValidVideoUrl(string? url)
        {
            return string.IsNullOrWhiteSpace(url) ||
                Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private ObjectResult? ValidateCover(IFormFile? cover)
        {
            if (cover is null)
            {
                return null;
            }

            if (cover.Length == 0)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid cover",
                    detail: "The cover file is empty.");
            }

            var extension = Path.GetExtension(cover.FileName).ToLowerInvariant();

            if (!AllowedCoverExtensions.Contains(extension))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid cover",
                    detail: "Only .jpg, .jpeg, .png and .webp images are allowed.");
            }

            if (cover.Length > MaxCoverBytes)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Cover too large",
                    detail: "The cover must be 5 MB or smaller.");
            }

            return null;
        }

        private async Task<string> BuildUniqueSlugAsync(
            string? requestedSlug,
            string title,
            string? excludeId,
            CancellationToken cancellationToken)
        {
            var baseSlug = SlugGenerator.Create(string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug);
            var candidate = baseSlug;
            var suffix = 2;

            while (await guideRepository.SlugExistsAsync(candidate, excludeId, cancellationToken))
            {
                candidate = $"{baseSlug}-{suffix++}";
            }

            return candidate.Length <= 200 ? candidate : candidate[..200].Trim('-');
        }

        private async Task<string> BuildUniqueCategorySlugAsync(
            string? requestedSlug,
            string name,
            string? excludeId,
            CancellationToken cancellationToken)
        {
            var baseSlug = SlugGenerator.Create(string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug);
            var candidate = baseSlug;
            var suffix = 2;

            while (await guideRepository.CategorySlugExistsAsync(candidate, excludeId, cancellationToken))
            {
                candidate = $"{baseSlug}-{suffix++}";
            }

            return candidate.Length <= 120 ? candidate : candidate[..120].Trim('-');
        }
    }
}
