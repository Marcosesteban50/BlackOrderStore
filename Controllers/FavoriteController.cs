using Microsoft.AspNetCore.Mvc;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class FavoriteController : ControllerBase
    {
        public FavoriteController()
        {


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NoContent();
        }
    }
    
}
