using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapibaraAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = "CapibarsScope")]
    public class Capibaras : Controller
    {
        /// <summary>
        /// Возвращает результат действия Index.
        /// </summary>
        /// <returns>Объект IActionResult, содержащий сообщение "Capibaras want to swim".</returns>
        public IActionResult Index()
        {
            return Ok("Capibaras want to swim");
        }
    }
}
