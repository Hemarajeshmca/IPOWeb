using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace IPOWeb.Controllers
{
    public class PanValidateController : Controller
    {
        public IActionResult PanValidate()
        {
            return View();
        }
        private IConfiguration _configuration;
        public PanValidateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";
    }
}
