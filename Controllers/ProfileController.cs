using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
  
    [ApiController]
    [Route("api/[Controller]")]
    public class ProfileController : ControllerBase
    {

        public ProfileController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }

    }
}
