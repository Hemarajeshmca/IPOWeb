using Microsoft.AspNetCore.Mvc;

namespace IPOWeb.Controllers
{
    public class ConnectorController : Controller
    {
        private readonly IConfiguration _configuration;
        public ConnectorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Connector()
        {
            ViewBag.HideIPO = true;
            var pipe = _configuration.GetValue<string>("AppSettings:connector");
            var user_code = _configuration.GetSection("AppSettings")["user_code"].ToString();
            ViewBag.pipe = pipe + "?user_code=" + user_code;
            return View();
        }
    }
}
