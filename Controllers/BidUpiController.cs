using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Data;

namespace IPOWeb.Controllers
{
    public class BidUpiController : Controller
    {
        public IActionResult BidUpi()
        {
            return View();
        }

        private IConfiguration _configuration;
        public BidUpiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpGet]
        public JsonResult getBidUpi(string offer_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getBidUpi";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = urlstring + "?offer_code=" + offer_code;
                    var response = client.GetAsync(url).Result;
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
                        var companyData = JsonConvert.DeserializeObject<object>(resultMessage);
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

        [HttpGet]
        public IActionResult getdetailUPISummary(string offer_code, string bank_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getBidUpidetail";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = urlstring + "?offer_code=" + offer_code + "&bank_code=" + bank_code;
                    var response = client.GetAsync(url).Result;
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
                        var jsonString = JsonConvert.DeserializeObject<string>(resultMessage);
                        var dataTable = JsonConvert.DeserializeObject<DataTable>(jsonString);

                        using (var workbook = new XLWorkbook())
                        {
                            workbook.Worksheets.Add(dataTable, "UPI Report");

                            using (var stream = new MemoryStream())
                            {
                                workbook.SaveAs(stream);
                                var content = stream.ToArray();
                                return File(
                                    content,
                                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                    "UPI_Detail_Report.xlsx"
                                );
                            }
                        }
                        // return Json(new { success = true, data = companyData });
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

        [HttpGet]
        public IActionResult getUPIdifferenceSummary(string offer_code, string user_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getUPIdifferenceSummary";
            DataSet result = new DataSet();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = urlstring + "?offer_code=" + offer_code + "&user_code=" + user_code;
                    var response = client.GetAsync(url).Result;
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

                        string d2 = JsonConvert.DeserializeObject<string>(resultMessage);
                        result = JsonConvert.DeserializeObject<DataSet>(d2);

                        using (var workbook = new XLWorkbook())
                        {
                            workbook.Worksheets.Add(result.Tables[0], "Sheet1");
                            using (var stream = new MemoryStream())
                            {
                                workbook.SaveAs(stream);
                                var content = stream.ToArray();
                                return File(
                                    content,
                                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                    "Bid_upi_difference.xlsx"
                                );

                            }
                        }
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


    }
}
