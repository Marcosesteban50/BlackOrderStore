using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class OrderController : ControllerBase
    {
        public OrderController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }
    }
}
