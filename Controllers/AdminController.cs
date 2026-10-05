using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AdminController : ControllerBase
    {
        public AdminController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }
    }
}
