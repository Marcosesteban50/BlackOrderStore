using Black_order.DTOs.Common;
using Black_order.DTOs.OrderDTOs;
using Black_order.Repositories.Orders;
using Black_order.Services.Orders;
using Black_order.Services.UserServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public sealed class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly IOrderRepository orderRepository;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public OrderController(
            IOrderService orderService,
            IOrderRepository orderRepository,
            IUserService userService,
            IMapper mapper)
        {
            this.orderService = orderService;
            this.orderRepository = orderRepository;
            this.userService = userService;
            this.mapper = mapper;
        }

        // POST: api/Order/Checkout
        [HttpPost("Checkout")]
        public async Task<ActionResult<OrderDTO>> Checkout(
            [FromBody] CheckoutDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = await userService.RequireUserId();
            var order = await orderService.CheckoutAsync(userId, dto, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/Order/MyOrders?page=1&pageSize=10&status=
        [HttpGet("MyOrders")]
        public async Task<ActionResult<PagedResult<OrderSummaryDTO>>> MyOrders(
            [FromQuery] OrderFilterDTO filter,
            CancellationToken cancellationToken)
        {
            var userId = await userService.RequireUserId();
            var (items, totalCount) = await orderRepository.GetByUserAsync(userId, filter, cancellationToken);

            return Ok(new PagedResult<OrderSummaryDTO>
            {
                Items = mapper.Map<List<OrderSummaryDTO>>(items),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            });
        }

        // GET: api/Order/Admin?page=1&pageSize=10&status=&search=
        [HttpGet("Admin")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<PagedResult<OrderSummaryDTO>>> All(
            [FromQuery] OrderFilterDTO filter,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await orderRepository.GetAllAsync(filter, cancellationToken);

            return Ok(new PagedResult<OrderSummaryDTO>
            {
                Items = mapper.Map<List<OrderSummaryDTO>>(items),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            });
        }

        // GET: api/Order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetById(string id, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
            {
                return NotFound();
            }

            var userId = await userService.RequireUserId();

            if (order.UserId != userId && !User.HasClaim(claim => claim.Type == "Admin"))
            {
                // Do not reveal that the order exists.
                return NotFound();
            }

            return Ok(mapper.Map<OrderDTO>(order));
        }

        // PATCH: api/Order/{id}/Status
        [HttpPatch("{id}/Status")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<OrderDTO>> UpdateStatus(
            string id,
            [FromBody] UpdateOrderStatusDTO dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return Ok(await orderService.UpdateStatusAsync(id, dto.Status, cancellationToken));
        }
    }
}
