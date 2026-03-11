using DocumentFormat.OpenXml.Bibliography;
using IPOWeb.Models;
using Microsoft.AspNetCore.Authorization;
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
    public class FileImportController : Controller
    {
        public IActionResult FileImport()
        {
            return View();
        }

        private IConfiguration _configuration;
        public FileImportController(IConfiguration configuration)
        {

            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpGet]
        public async Task<JsonResult> GetDatasetList()
        {
            urlstring = _configuration.GetSection("Appsettings")["connector_api"] + "Pipeline/GetDataset_list";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(urlstring);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<FileImportModel>>(responseString);
                    return Json(new
                    {
                        success = true,
                        data = result
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

        [HttpPost]
        public async Task<JsonResult> GetPipelinetList(string dataset)
        {
            urlstring = _configuration.GetSection("Appsettings")["connector_api"]
                        + "getPipelinelistData?dataset=" + dataset;
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(urlstring);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<FileImportModel>>(responseString);
                    return Json(new
                    {
                        success = true,
                        data = result
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
