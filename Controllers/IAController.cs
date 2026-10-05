using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class IAController : ControllerBase
    {
        public IAController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }
    }
}
