using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapibaraAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = "CapibarsScope")]
    public class Capibaras : Controller
    {
        
        public IActionResult Index()
        {
            return Ok("Capibaras want to swim");
        }
    }
}
