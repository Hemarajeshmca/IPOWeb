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
                        + "Pipeline/getPipelinelistData?dataset=" + dataset;
            try
            {
                using (var client = new HttpClient())                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization =   new AuthenticationHeaderValue("Bearer", token);
                    // ✅ POST call instead of GET
                    var response = await client.PostAsync(urlstring, null);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<PipelineModel>>(responseString);
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
        public async Task<JsonResult> ImportData(
     IFormFile file,
     string pipeline_code,
     string initiated_by,
     string dataset_code,
     string reference_no)
        {
            try
            {
                if (file == null)
                {
                    return Json(new { success = false, message = "File not received" });
                }

                string urlstring = _configuration.GetSection("Appsettings")["connector_api"]
                    + "Pipeline/NewScheduler?pipeline_code=" + pipeline_code
                    + "&initiated_by=" + initiated_by
                    + "&dataset_code=" + dataset_code
                    + "&reference_no=" + reference_no;

                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    APIcookieName = "APItoken-" + User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                                  + "_" + User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[APIcookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    using (var content = new MultipartFormDataContent())
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var fileContent = new StreamContent(stream);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                            content.Add(fileContent, "file", file.FileName);

                            var response = await client.PostAsync(urlstring, content);

                            var responseString = await response.Content.ReadAsStringAsync();

                            return Json(new
                            {
                                success = response.IsSuccessStatusCode,
                                data = responseString
                            });
                        }
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
    }
}
