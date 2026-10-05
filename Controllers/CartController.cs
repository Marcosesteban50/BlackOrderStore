using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CartController : ControllerBase
    {
        public CartController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }
    }
}
