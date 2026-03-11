using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace IPOWeb.Controllers
{
    public class ClientSetupController : Controller
    {      
        public IActionResult ClientSetup(string mode)
        {
            ViewBag.HideIPO = true;
            ViewBag.Mode = mode;
            ViewBag.Status = "Draft";                // 👈 Draft for create
            ViewBag.ClientCode = "<<Client Code>>"; // 👈 Default placeholder
            return View();
        }


        private IConfiguration _configuration;
        public ClientSetupController(IConfiguration configuration)
        {

            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpPost]
        public JsonResult ClientType([FromBody] ClientSetupModel mymodel)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "/ClientType";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(mymodel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = client.PostAsync(urlstring, content).Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string resultMessage = response.Content.ReadAsStringAsync().Result;

                        // Deserialize the JSON string into an object
                        var companyData = JsonConvert.DeserializeObject<object>(resultMessage);

                        // Return that object directly, not as a string
                        return Json(new { success = true, data = companyData });
                    }


                    else
                    {
                        return Json(new { success = false, message = "API call failed: " + response.StatusCode });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult GetClientList()
        {
            var data = new List<object>
            {
                new { Id = 1,IssueCode = "CLIPO001", ClientName = "LG Electronics", CIN = "L01631KA2010PTC096843", City = "Chennai", State = "Tamilnadu", ClientStatus = "Active" },
                new { Id = 2, IssueCode = "CLIPO002", ClientName = "ICICI Prudential", CIN = "U12345DL2022PTC123456", City = "Kochi", State = "Kerala",  ClientStatus = "Inactive" },
                new { Id = 3, IssueCode = "CLIPO003", ClientName = "HBD Finance Groups", CIN = "L21091MH2022OPC141331", City = "Bangalore",  State = "Karnataka", ClientStatus = "Active" }
            };

            return Json(data);
        }
        

        [HttpPost]
        public IActionResult ClientSetupList(int id)
        {
            ViewBag.Mode = "Edit";

            var data = new List<dynamic>
                {
                    new { Id = 1, ClientCode = "C001", IssueStatus = "Active", Person = "LG Swamyvel",Mob = "9887563425",
                        Country = "India", Email = "swamy@gmail.com", Address = "Lloyds Road, Chennai", State = "TN", City = "CH", PIN = "600909",
                        ClientName = "LG Electronics",  ClientType = "PL", CIN = "L01631KA2010PTC096843" },
                    new { Id = 2, ClientCode = "C002", IssueStatus = "Inactive", Person = "Raj Kumar",Mob = "9887563245",
                        Country = "India", Email = "raj@gmail.com", Address = "Haddows Road", State = "KR", City = "TR", PIN = "700909",
                        ClientName = "ICICI Prudential", ClientType = "PU", CIN = "U12345DL2022PTC123456" },
                    new { Id = 3, ClientCode = "C003", IssueStatus = "Active", Person = "Sam",Mob = "98875345425",
                        Country = "India", Email = "sam@gmail.com", Address = "Mint Road", State = "KA", City = "MD", PIN = "800909",
                        ClientName = "HBD Finance Groups", ClientType = "PR", CIN = "L21091MH2022OPC141331"},
                };

            var client = data.FirstOrDefault(x => x.Id == id);

            if (client != null)
            {
                ViewBag.ClientCode = client.ClientCode;
                ViewBag.Status = client.IssueStatus;
                ViewBag.ClientName = client.ClientName;
                ViewBag.CIN = client.CIN;
                ViewBag.ClientType = client.ClientType;
                ViewBag.Person = client.Person;
                ViewBag.Mob = client.Mob;
                ViewBag.Email = client.Email;
                ViewBag.Address = client.Address;
                ViewBag.State = client.State;
                ViewBag.City = client.City;
                ViewBag.PIN = client.PIN;
                ViewBag.Country = client.Country;
            }

            return View("ClientSetup");   // 👈 IMPORTANT
        }
    }
}
