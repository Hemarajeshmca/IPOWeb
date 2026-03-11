using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using IPOWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace IPOWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private IConfiguration _configuration;
        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";
        public IActionResult Dashboard()
        {
            string name = User.FindFirst(ClaimTypes.Name)?.Value.ToString();
            string role = User.FindFirst(ClaimTypes.Role)?.Value.ToString();
            HttpContext.Session.SetString("user_id","1");
            return View();
        }

        public async Task<JsonResult> GetClientList([FromBody] clientList mymodel)
        {
            var urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "GetClientList";
            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(mymodel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(urlstring, content);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string _data1 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data1);
                    string _data = JsonConvert.SerializeObject(result.Tables[0]);
                    return Json(new { _data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
