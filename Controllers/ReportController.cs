using Microsoft.AspNetCore.Mvc;

namespace IPOWeb.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Report()
        {
            return View();
        }
    }
}
