using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
                        string templatePath = @"C:\Users\emp10174\Desktop\simple_boa_report_template2.xlsx";
                        string outputPath = @"E:\IPO_MOM\BOA_Report.xlsx";
                        System.IO.File.Copy(templatePath, outputPath, true);
                        using (XLWorkbook wb = new XLWorkbook(outputPath))
                        {
                            DataTable dtOfferDetails = result.Tables[0];
                            DataTable dtRetail = result.Tables[1];
                            DataTable dtemp = result.Tables[3];
                            DataTable dtCO = result.Tables[4];
                            DataTable dtQIB = result.Tables[5];
                            DataTable dtNRB10L = result.Tables[6];
                            DataTable dtNRA10L = result.Tables[7];
                            DataTable dtMM = result.Tables[8];

                            var celA2 = "Public Issue of "
                                        + result.Tables[0].Rows[0]["Number of Shares"].ToString()
                                        + " equity shares of Rs. "
                                        + result.Tables[0].Rows[0]["Face value Rs."].ToString()
                                        + "/- each issued for cash at a price of Rs. "
                                        + result.Tables[0].Rows[0]["Issue Price Rs."].ToString()
                                        + " per share";

                            WriteToSheet(wb, "Data", dtOfferDetails, celA2);
                            WriteToSheet(wb, "retail_data", dtRetail);
                            WriteToSheet(wb, "NRA10L_data", dtNRB10L);
                            WriteToSheet(wb, "NRB10L_data", dtNRB10L);
                            WriteToSheet(wb, "MM_data", dtMM);
                            WriteToSheet(wb, "QIB_data", dtQIB);
                            wb.Save();
                        }
                        byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);

                        return File(
                            fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "BOA_Report.xlsx"
                        );
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


        [HttpGet]
        public JsonResult getMomReports(string offer_code)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getMomReports";
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

                        var temp = JsonConvert.DeserializeObject<dynamic>(resultMessage);

                        if (temp is string)
                        {
                            temp = JsonConvert.DeserializeObject<dynamic>(temp);
                        }

                        // Convert Table → List<Dictionary>
                        var table1Data = ((IEnumerable<dynamic>)temp.Table)
                             .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                             .ToObject<Dictionary<string, object>>())
                             .ToList();

                        var table2Data = ((IEnumerable<dynamic>)temp.Table1)
                            .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                            .ToObject<Dictionary<string, object>>())
                            .ToList();

                        var table3Data = ((IEnumerable<dynamic>)temp.Table2)
                            .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                            .ToObject<Dictionary<string, object>>())
                            .ToList();

                        var table4Data = ((IEnumerable<dynamic>)temp.Table3)
                           .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                           .ToObject<Dictionary<string, object>>())
                           .ToList();

                        var table5Data = ((IEnumerable<dynamic>)temp.Table4)
                          .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                          .ToObject<Dictionary<string, object>>())
                          .ToList();

                        var table6Data = ((IEnumerable<dynamic>)temp.Table5)
                          .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                          .ToObject<Dictionary<string, object>>())
                          .ToList();

                        var table7Data = ((IEnumerable<dynamic>)temp.Table6)
                          .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                          .ToObject<Dictionary<string, object>>())
                          .ToList();

                        var table8Data = ((IEnumerable<dynamic>)temp.Table7)
                          .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                          .ToObject<Dictionary<string, object>>())
                          .ToList();

                        var table9Data = ((IEnumerable<dynamic>)temp.Table8)
                         .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                         .ToObject<Dictionary<string, object>>())
                         .ToList();

                        var table10Data = ((IEnumerable<dynamic>)temp.Table9)
                        .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                        .ToObject<Dictionary<string, object>>())
                        .ToList();

                        var table11Data = ((IEnumerable<dynamic>)temp.Table10)
                        .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                        .ToObject<Dictionary<string, object>>())
                        .ToList();

                        return Json(new
                        {
                            success = true,
                            data = new
                            {
                                table1 = table1Data,
                                table2 = table2Data,
                                table3 = table3Data,
                                table4 = table4Data,
                                table5 = table5Data,
                                table6 = table6Data,
                                table7 = table7Data,
                                table8 = table8Data,
                                table9 = table9Data,
                                table10 = table10Data,
                                table11 = table11Data,
                            }
                        });
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

        [HttpPost("generateReport")]
        public IActionResult generateReport([FromBody] MomRequest data)
        {
            var summary = data.summary;
            var bankData = data.bankData;
            var nonasbabankData = data.nonasbabankData;
            var rejectionData = data.rejectionData;
            var categoryData = data.categoryData;
            var categoryINDData = data.categoryINDData;
            var CategoryCo = data.categoryCo;
            var CategoryNRA10L = data.categoryNRA10L;
            var CategoryNRB10L = data.categoryNRB10L;
            var BankUPIData = data.bankUPIData;
            var bidApplRcd = data.bidApplRcd;

            string clientName = summary.client_name;
            int offer_issuesize = summary.offer_issuesize;
            int offer_facevalue = summary.offer_facevalue;
            int offer_premiun = summary.offer_premiun;
            int offer_fixedprice = summary.offer_fixedprice;
            int total_iposize = summary.total_iposize;
            int mm_shares = summary.mm_shares;
            int total_mm = summary.total_mm;
            int public_shares = summary.public_shares;
            int net_issue = summary.net_issue;
            long asba_total_bids = bidApplRcd.asba_total_bids;
            long asba_total_quantity = bidApplRcd.asba_total_quantity;
            long nonasba_total_bids = bidApplRcd.nonasba_total_bids;
            long nonasba_total_quantity = bidApplRcd.nonasba_total_quantity;
            long upi_total_bids = bidApplRcd.upi_total_bids;
            long upi_total_quantity = bidApplRcd.upi_total_quantity;
            long total_bids = bidApplRcd.total_bids;
            long total_quantity = bidApplRcd.total_quantity;
            long diff_bids = bidApplRcd.diff_bids;
            long diff_quantity = bidApplRcd.diff_quantity;
            long diff1 = bidApplRcd.total_bids - bidApplRcd.diff_bids;
            long diff2 = bidApplRcd.total_quantity - bidApplRcd.diff_quantity;
            long banknotbidbids = bidApplRcd.banknotbidbids;
            long banknotbidqty = bidApplRcd.banknotbidqty;
            long bank_bids = bidApplRcd.bank_bids;
            long bank_bids_qty = bidApplRcd.bank_bids_qty;

            string templatePath = _configuration["Appsettings:templatePath"];
            string generatedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Generated");

            if (!Directory.Exists(generatedFolder))
                Directory.CreateDirectory(generatedFolder);

            string fileName = "Mom_Report_" + DateTime.Now.Ticks + ".docx";
            string newFilePath = Path.Combine(generatedFolder, fileName);

            System.IO.File.Copy(templatePath, newFilePath, true);

            using (WordprocessingDocument doc = WordprocessingDocument.Open(newFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Replace placeholders
                ReplaceText(body, "{client_name}", clientName);
                ReplaceText(body, "{offer_issuesize}", offer_issuesize.ToString("N0"));
                ReplaceText(body, "{offer_facevalue}", offer_facevalue.ToString("N0"));
                ReplaceText(body, "{offer_premiun}", offer_premiun.ToString("N0"));
                ReplaceText(body, "{offer_fixedprice}", offer_fixedprice.ToString("N0"));
                ReplaceText(body, "{total_iposize}", total_iposize.ToString("N0"));
                ReplaceText(body, "{total_mm}", total_mm.ToString("N0"));
                ReplaceText(body, "{mm_shares}", mm_shares.ToString("N0"));
                ReplaceText(body, "{public_shares}", public_shares.ToString("N0"));
                ReplaceText(body, "{net_issue}", net_issue.ToString("N0"));
                ReplaceText(body, "{asba_total_bids}", asba_total_bids.ToString("N0"));
                ReplaceText(body, "{asba_total_quantity}", asba_total_quantity.ToString("N0"));
                ReplaceText(body, "{nonasba_total_bids}", nonasba_total_bids.ToString("N0"));
                ReplaceText(body, "{nonasba_total_quantity}", nonasba_total_quantity.ToString("N0"));
                ReplaceText(body, "{upi_total_bids}", upi_total_bids.ToString("N0"));
                ReplaceText(body, "{upi_total_quantity}", upi_total_quantity.ToString("N0"));
                ReplaceText(body, "{total_bids}", total_bids.ToString("N0"));
                ReplaceText(body, "{total_quantity}", total_quantity.ToString("N0"));
                ReplaceText(body, "{diff_bids}", diff_bids.ToString("N0"));
                ReplaceText(body, "{diff_quantity}", diff_quantity.ToString("N0"));
                ReplaceText(body, "{diff1}", diff1.ToString("N0"));
                ReplaceText(body, "{diff2}", diff2.ToString("N0"));
                ReplaceText(body, "{banknotbidbids}", banknotbidbids.ToString("N0"));
                ReplaceText(body, "{banknotbidqty}", banknotbidqty.ToString("N0"));
                ReplaceText(body, "{bank_bids}", bank_bids.ToString("N0"));
                ReplaceText(body, "{bank_bids_qty}", bank_bids_qty.ToString("N0"));

                var para = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
    .FirstOrDefault(p => p.InnerText.Contains("Net Collections of Non-institutional and Individual Investor Categories by ASBA"));

                if (para != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table bankTable = null;

                    var next = para.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            bankTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (bankTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = bankTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            bankTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var bank in bankData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(bank.bank_name.ToUpper(), true, false),
                                CreateCell(bank.bnk_appl_count.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(bank.bnk_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(bank.bank_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            bankTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(bankData.Sum(x => x.bnk_appl_count).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(bankData.Sum(x => x.bnk_quantity).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(bankData.Sum(x => x.bank_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        bankTable.Append(totalRow);
                    }
                }

                var paras = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("Net Collections of Non-institutional and Individual Investor Categories by SYNDICATE ASBA:"));

                if (paras != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table nonasbabankTable = null;

                    var next = paras.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            nonasbabankTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (nonasbabankTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = nonasbabankTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            nonasbabankTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var nonasbabank in nonasbabankData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(nonasbabank.bank_name.ToUpper(), true, false),
                                CreateCell(nonasbabank.bnk_appl_count.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(nonasbabank.bnk_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(nonasbabank.bank_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            nonasbabankTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(nonasbabankData.Sum(x => x.bnk_appl_count).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(nonasbabankData.Sum(x => x.bnk_quantity).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(nonasbabankData.Sum(x => x.bank_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        nonasbabankTable.Append(totalRow);
                    }
                }

                var para1 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("the bid book under the various heads are as mentioned below"));

                if (para1 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table rejectionTable = null;

                    var next = para1.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            rejectionTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (rejectionTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = rejectionTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            rejectionTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var rejection in rejectionData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(rejection.rejected_reason.ToUpper(), true, false),
                                CreateCell(rejection.rejection_count.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(rejection.total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true)
                            );

                            rejectionTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("TOTAL", true, false),
                            CreateCell(rejectionData.Sum(x => x.rejection_count).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(rejectionData.Sum(x => x.total_quantity).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        rejectionTable.Append(totalRow);
                    }
                }

                var para2 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("the applications processed by Registrar after rejecting invalid bids and bids not banked are as under:"));

                if (para2 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table categoryTable = null;

                    var next = para2.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            categoryTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (categoryTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = categoryTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            categoryTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var category in categoryData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(category.ipo_category.ToUpper(), true, false),
                                CreateCell(category.total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(category.quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(category.total.ToString("N2", new CultureInfo("en-IN")), true, true) //
                            );

                            categoryTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(categoryData.Sum(x => x.total_appl).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(categoryData.Sum(x => x.quantity).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(categoryData.Sum(x => x.total).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        categoryTable.Append(totalRow);
                    }
                }

                var para3 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("B: 1- Retail Individual Investors Category (For 2 Lot)"));

                if (para3 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catIndTable = null;

                    var next = para3.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catIndTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catIndTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catIndTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catIndTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var catInd in categoryINDData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(catInd.total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(catInd.total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(catInd.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true)
                            );

                            catIndTable.Append(row);
                            srNo++;
                        }
                    }
                }

                var para4 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("CO Allotments"));

                if (para4 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catCoTable = null;

                    var next = para4.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catCoTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catCoTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catCoTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catCoTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var catCo in CategoryCo)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(catCo.total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(catCo.total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(catCo.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true)
                            );

                            catCoTable.Append(row);
                            srNo++;
                        }
                    }
                }

                var para5 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("B: 3- Other than Retail Individual Investors (above 2 Lots and Share Apply Amount > 1000000)"));

                if (para5 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catNRA10LTable = null;

                    var next = para5.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catNRA10LTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catNRA10LTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catNRA10LTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catNRA10LTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryNRA10L in CategoryNRA10L)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categoryNRA10L.total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRA10L.total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRA10L.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true)
                            );

                            catNRA10LTable.Append(row);
                            srNo++;
                        }
                    }
                }

                var para6 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("B: 2- Other than Retail Individual Investors (above 2 Lots and Share Apply Amount <= 1000000)"));

                if (para6 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catNRB10LTable = null;

                    var next = para6.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catNRB10LTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catNRB10LTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catNRB10LTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catNRB10LTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryNRB10L in CategoryNRB10L)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categoryNRB10L.total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRB10L.total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRB10L.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true)
                            );

                            catNRB10LTable.Append(row);
                            srNo++;
                        }
                    }
                }

                var para7 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("Net Collections of Non-institutional and Individual Investor Categories (UPI)"));

                if (para7 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table bankUPITable = null;

                    var next = para7.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            bankUPITable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (bankUPITable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = bankUPITable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            bankUPITable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var bankUPI in BankUPIData)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(bankUPI.bank_name.ToUpper(), true, false),
                                CreateCell(bankUPI.no_of_bids.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(bankUPI.no_of_shares_applied.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(bankUPI.total_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            bankUPITable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(BankUPIData.Sum(x => x.no_of_bids).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(BankUPIData.Sum(x => x.no_of_shares_applied).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(BankUPIData.Sum(x => x.total_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        bankUPITable.Append(totalRow);
                    }
                }

                // 🔥 NEXT STEP: Bank Table (you will add here)

                doc.MainDocumentPart.Document.Save();
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(newFilePath);
            Response.Headers.Add("Content-Disposition", "attachment; filename=MOM_Report.docx");

            return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                );
        }

        private void ReplaceText(Body body, string placeholder, string newValue)
        {
            foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                var texts = para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();

                string fullText = string.Concat(texts.Select(t => t.Text));

                if (fullText.Contains(placeholder))
                {
                    fullText = fullText.Replace(placeholder, newValue ?? "");

                    // update ONLY first text node to preserve formatting
                    texts.First().Text = fullText;

                    // clear remaining split parts
                    for (int i = 1; i < texts.Count; i++)
                    {
                        texts[i].Text = "";
                    }
                }
            }
        }

        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateCell(string text, bool isBold, bool isRightAlign)
        {
            var run = new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Text(text)
            );

            if (isBold)
                run.RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.Bold()
                );

            var paraProps = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

            if (isRightAlign)
                paraProps.Justification = new DocumentFormat.OpenXml.Wordprocessing.Justification()
                {
                    Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Right
                };

            var para = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(paraProps, run);

            return new DocumentFormat.OpenXml.Wordprocessing.TableCell(para);
        }
    }
}

