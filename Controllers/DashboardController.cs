using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using IPOWeb.Models;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace IPOWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            string name = User.FindFirst(ClaimTypes.Name)?.Value.ToString();
            string role = User.FindFirst(ClaimTypes.Role)?.Value.ToString();
            HttpContext.Session.SetString("user_id","1");
            return View();
        }
    }
}
