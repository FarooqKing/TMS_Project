using Microsoft.AspNetCore.Mvc;

namespace TMS_Project.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/403")]
        public IActionResult Forbidden()
        {
            return View();
        }
    }
}
