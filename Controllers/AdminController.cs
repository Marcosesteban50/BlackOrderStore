using Black_order.DTOs.AdminBoardDTOs;
using Black_order.DTOs.Common;
using Black_order.DTOs.ProductDTOs;
using Black_order.Repositories.Admin;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize(Policy = "Admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly IAdminRepository adminRepository;
        private readonly IMapper mapper;

        public AdminController(IAdminRepository adminRepository, IMapper mapper)
        {
            this.adminRepository = adminRepository;
            this.mapper = mapper;
        }

        // GET: api/Admin/Dashboard
        [HttpGet("Dashboard")]
        public async Task<ActionResult<AdminDashboardDTO>> Dashboard(CancellationToken cancellationToken)
        {
            return Ok(await adminRepository.GetDashboardAsync(cancellationToken));
        }

        // GET: api/Admin/Products/Pending
        [HttpGet("Products/Pending")]
        public async Task<ActionResult<PagedResult<ProductDTO>>> PendingProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var (items, totalCount) = await adminRepository.GetPendingProductsAsync(
                page,
                pageSize,
                cancellationToken);

            return Ok(new PagedResult<ProductDTO>
            {
                Items = mapper.Map<List<ProductDTO>>(items),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }

        // POST: api/Admin/Products/{id}/Approve
        [HttpPost("Products/{id}/Approve")]
        public Task<IActionResult> Approve(string id, CancellationToken cancellationToken)
            => SetApprovalAsync(id, approved: true, cancellationToken);

        // POST: api/Admin/Products/{id}/Reject
        [HttpPost("Products/{id}/Reject")]
        public Task<IActionResult> Reject(string id, CancellationToken cancellationToken)
            => SetApprovalAsync(id, approved: false, cancellationToken);

        private async Task<IActionResult> SetApprovalAsync(
            string id,
            bool approved,
            CancellationToken cancellationToken)
        {
            var product = await adminRepository.GetTrackedProductAsync(id, cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            product.Approved = approved;
            await adminRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
