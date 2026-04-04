using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;

namespace IPOWeb.Controllers
{
    public class BOAController : Controller
    {
        public IActionResult BOA()
        {
            return View();
        }

        private IConfiguration _configuration;
        public BOAController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpGet]
        public JsonResult getboalist(string offer_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getboalist";
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

        // getboareport
        [HttpGet]
        public IActionResult getboareport(string offer_code)
        {
            string post_data = "";
            DataSet result = new DataSet();
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getboareport";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                    string token = Request.Cookies[APIcookieName];

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = urlstring + "?offer_code=" + offer_code;
                    var response = client.GetAsync(url).Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new { success = false, authExpired = true });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string resultMessage = response.Content.ReadAsStringAsync().Result;

                        string d2 = JsonConvert.DeserializeObject<string>(resultMessage);
                        result = JsonConvert.DeserializeObject<DataSet>(d2);
                        DataTable dtNames = result.Tables[0];
                        string templatePath = @"C:\Users\emp10176\Desktop\simple_boa_report_template1.xlsx";
                        string outputPath = @"E:\user\hema\IPOProject\Outputfiles\BOA_Report.xlsx";

                        System.IO.File.Copy(templatePath, outputPath, true);

                        using (XLWorkbook wb = new XLWorkbook(outputPath))
                        {
                            DataTable dtOfferDetails = result.Tables[0];
                            DataTable dtRetail = result.Tables[1];
                            DataTable dtHNI = result.Tables[2]; // example

                            var celA2 = "Public Issue of "
                                        + result.Tables[0].Rows[0]["Number of Shares"].ToString()
                                        + " equity shares of Rs. "
                                        + result.Tables[0].Rows[0]["Face value Rs."].ToString()
                                        + "/- each issued for cash at a price of Rs. "
                                        + result.Tables[0].Rows[0]["Issue Price Rs."].ToString()
                                        + " per share";

                            // Pass the text here
                            WriteToSheet(wb, "Data", dtOfferDetails, celA2);
                            WriteToSheet(wb, "retail_data", dtRetail);
                            WriteToSheet(wb, "nonretail_data", dtHNI);

                            wb.Save();
                        }
                        byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);

                        return File(
                            fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "BOA_Report.xlsx"
                        );

                        /* write multiplesheet  command by hema */
                        //using (XLWorkbook wb = new XLWorkbook())
                        //{
                        //    HashSet<string> usedNames = new HashSet<string>();

                        //    // ✅ Step 1: Create sheets based on cat_name
                        //    foreach (DataRow row in dtNames.Rows)
                        //    {
                        //        string sheetName = row["cat_name"].ToString();

                        //        // ✅ Handle empty
                        //        if (string.IsNullOrWhiteSpace(sheetName))
                        //            sheetName = "Sheet" + (dtNames.Rows.IndexOf(row) + 1);

                        //        // ✅ Clean invalid characters
                        //        sheetName = sheetName.Replace("/", "-")
                        //                             .Replace("\\", "-")
                        //                             .Replace("*", "")
                        //                             .Replace("?", "")
                        //                             .Replace("[", "")
                        //                             .Replace("]", "");

                        //        if (sheetName.Length > 31)
                        //            sheetName = sheetName.Substring(0, 31);

                        //        // ✅ Avoid duplicate names
                        //        string original = sheetName;
                        //        int i = 1;
                        //        while (usedNames.Contains(sheetName))
                        //        {
                        //            sheetName = original + "_" + i++;
                        //        }
                        //        usedNames.Add(sheetName);

                        //        // ✅ Create EMPTY sheet
                        //        wb.Worksheets.Add(sheetName);
                        //    }

                        //    // 👉 Step 2 (later): you can write data like this
                        //    /*
                        //    var ws = wb.Worksheet("Retail");
                        //    ws.Cell(1,1).Value = "Your Data";
                        //    */

                        //    using (MemoryStream stream = new MemoryStream())
                        //    {
                        //        wb.SaveAs(stream);
                        //        stream.Position = 0;

                        //        return File(
                        //            stream.ToArray(),
                        //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        //            "BOA_Report.xlsx"
                        //        );
                        //    }
                        //}
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

        void WriteToSheet(XLWorkbook wb, string sheetName, DataTable dt, string topText = null)
        {
            // ✅ Check if sheet exists
            if (!wb.Worksheets.Any(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase)))
            {
                return; // 🚫 Do nothing if sheet not found
            }

            var ws = wb.Worksheet(sheetName);

            int currentRow = 1;

            // Optional: clear existing content
            ws.Clear();

            // ✅ Top text
            if (!string.IsNullOrEmpty(topText))
            {
                ws.Cell(currentRow, 1).Value = topText;
                ws.Range(currentRow, 1, currentRow, dt.Columns.Count).Merge();
                currentRow++;
            }

            // ✅ Header
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ws.Cell(currentRow, i + 1).Value = dt.Columns[i].ColumnName;
            }
            currentRow++;

            // ✅ Data
            ws.Cell(currentRow, 1).InsertData(dt.Rows);
        }
    }
}
