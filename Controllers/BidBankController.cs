using ClosedXML.Excel;
using IPOWeb.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using PdfDocument = iTextSharp.text.Document;
using PdfFont = iTextSharp.text.Font;
using PdfFontFactory = iTextSharp.text.FontFactory;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;
using Microsoft.AspNetCore.Hosting;

namespace IPOWeb.Controllers
{
    public class BidBankController : Controller
    {
        public IActionResult BidBank()
        {
            return View();
        }

        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public BidBankController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpGet]
        public JsonResult getBidBank(string offer_code, string category, string recontype)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "GetbidBank";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //string url = urlstring + "?offer_code=" + offer_code;
                    string url = urlstring +
                         "?offer_code=" + offer_code +
                         "&category=" + Uri.EscapeDataString(category) +
                         "&recontype=" + recontype;
                    var response = client.GetAsync(url).Result;
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);
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
        public IActionResult getdetailBankSummary(string offer_code, string bank_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "GetbidBankdetail";
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
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);
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
                            workbook.Worksheets.Add(dataTable, "Bank Report");

                            using (var stream = new MemoryStream())
                            {
                                workbook.SaveAs(stream);
                                var content = stream.ToArray();
                                return File(
                                    content,
                                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                    "Bank_Report.xlsx"
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


        // getdetaildifferenceSummary

        [HttpGet]
        public IActionResult getdetaildifferenceSummary(string offer_code, string user_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getdetaildifferenceSummary";
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
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);
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
                                    "Bid_bank_difference.xlsx"
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



    public class BankReconModel
    {
        public string BankName { get; set; }
        public string Status { get; set; }

        // As per Bid
        public int BidNoOfAppl { get; set; }
        public int BidNoOfShares { get; set; }
        public decimal BidAmount { get; set; }

        // As per Bank
        public int BankNoOfAppl { get; set; }
        public int BankNoOfShares { get; set; }
        public decimal BankAmount { get; set; }

        // Difference
        public int DiffNoOfAppl { get; set; }
        public int DiffNoOfShares { get; set; }
        public decimal DiffAmount { get; set; }
    } 

    
}
