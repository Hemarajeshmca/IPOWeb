using DocumentFormat.OpenXml.Bibliography;
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
    public class ConfigurationController : Controller
    {
        private IConfiguration _configuration;
        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string APIcookieName = "";
        string urlstring = "";
        public IActionResult Configuration()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetConfigList()
        {
            string urlstring =  Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"])+ "fetchconfig";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" +User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // POST API call - no request body required
                    var response = await client.PostAsync(urlstring, null);
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext,response,APIcookieName);
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
                        string resultMessage = await response.Content.ReadAsStringAsync();
                        var companyData = JsonConvert.DeserializeObject<object>(resultMessage);
                        return Json(new
                        {
                            success = true,
                            data = companyData
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "API call failed: " + response.StatusCode
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateConfig([FromBody] ConfigurationModel objcmodel)
        {
            string urlstring =
                Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"])
                + "updateconfig";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    APIcookieName =
                        "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value.ToString() +
                        "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value.ToString();

                    string token = Request.Cookies[APIcookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );

                    // Convert model to JSON
                    string json = JsonConvert.SerializeObject(objcmodel);

                    // Create request content
                    var content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                    Console.WriteLine("UPDATE JSON: " + json);

                    // POST JSON to API
                    var response = await client.PostAsync(urlstring, content);

                    ApiTokenRefreshMiddleware.TokenUpdate(
                        HttpContext,
                        response,
                        APIcookieName
                    );

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
                        string resultMessage =
                            await response.Content.ReadAsStringAsync();

                        var companyData =
                            JsonConvert.DeserializeObject<object>(resultMessage);

                        return Json(new
                        {
                            success = true,
                            data = companyData
                        });
                    }

                    return Json(new
                    {
                        success = false,
                        message = "API call failed: " + response.StatusCode
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
