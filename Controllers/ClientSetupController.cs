using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
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
        public JsonResult ClientType([FromBody] Qcdgridread objgridread)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "ClientType";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(objgridread);
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

        [HttpPost]
        public async Task<JsonResult> IudClient([FromBody] clientDetails mymodel)
    {
        var urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "IudClient";
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
