// Controllers/HealthController.cs
using Microsoft.AspNetCore.Mvc;

namespace KareClass.Controllers
{
    [Route("health")]
    [ApiController]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy" });
        }
    }
}