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
using System.IO.Compression;


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

        private void GenerateExcel(string offer_code, string excelPath)
        {
            DataSet result = new DataSet();
            string urlstring = _configuration["Appsettings:apiurl"] + "getboareport";

            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;

                string APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                string token = Request.Cookies[APIcookieName];

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = urlstring + "?offer_code=" + offer_code;
                var response = client.GetAsync(url).Result;

                // ✅ FIX 1: Unauthorized handling
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Response.Cookies.Delete(APIcookieName);
                    throw new UnauthorizedAccessException("Token expired");
                }

                if (!response.IsSuccessStatusCode)
                    throw new Exception("API failed: " + response.StatusCode);

                string resultMessage = response.Content.ReadAsStringAsync().Result;
                string d2 = JsonConvert.DeserializeObject<string>(resultMessage);
                result = JsonConvert.DeserializeObject<DataSet>(d2);
            }

            // ✅ FIX 2: Validate data
            if (result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                throw new Exception("No data returned from API");

            string templatePath = @"C:\Users\emp10174\Desktop\simple_boa_report_template2.xlsx";
            System.IO.File.Copy(templatePath, excelPath, true);

            using (XLWorkbook wb = new XLWorkbook(excelPath))
            {
                DataTable dtOfferDetails = result.Tables[0];
                DataTable dtRetail = result.Tables[1];
                DataTable dtCO = result.Tables[4];
                DataTable dtQIB = result.Tables[5];
                DataTable dtNRB10L = result.Tables[2];
                DataTable dtNRA10L = result.Tables[3];
                DataTable dtMM = result.Tables[6];
                DataTable dtEmp = result.Tables[7];

                var celA2 = "Public Issue of "
                    + dtOfferDetails.Rows[0]["Number of Shares"]
                    + " equity shares of Rs. "
                    + dtOfferDetails.Rows[0]["Face value Rs."]
                    + "/- each issued for cash at a price of Rs. "
                    + dtOfferDetails.Rows[0]["Issue Price Rs."]
                    + " per share";

                WriteToSheet(wb, "Data", dtOfferDetails, celA2);
                WriteToSheet(wb, "retail_data", dtRetail);
                WriteToSheet(wb, "NRA10L_data", dtNRA10L);
                WriteToSheet(wb, "NRB10L_data", dtNRB10L);
                WriteToSheet(wb, "MM_data", dtMM);
                WriteToSheet(wb, "QIB_data", dtQIB);
                WriteToSheet(wb, "co_data", dtCO);

                // Hide sheets
                wb.Worksheet("retail_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("NRA10L_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("NRB10L_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("MM_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("QIB_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("co_data").Visibility = XLWorksheetVisibility.Hidden;
                wb.Worksheet("Data").Visibility = XLWorksheetVisibility.Hidden;

                HandleSheet(wb, "RETAIL", dtRetail);
                HandleSheet(wb, "NRA10L", dtNRA10L);
                HandleSheet(wb, "NRB10L", dtNRB10L);
                HandleSheet(wb, "Market Maker", dtMM);
                HandleSheet(wb, "QIB", dtQIB);
                HandleSheet(wb, "Corporate", dtCO);

                wb.Save();
            }
        }

        void HandleSheet(XLWorkbook wb, string sheetName, DataTable dt)
        {
            //  WriteToSheet(wb, sheetName, dt);

            if (dt == null || dt.Rows.Count == 0)
            {
                wb.Worksheet(sheetName).Hide();
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

                        var table12Data = ((IEnumerable<dynamic>)temp.Table11)
                        .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                        .ToObject<Dictionary<string, object>>())
                        .ToList();

                        var table13Data = ((IEnumerable<dynamic>)temp.Table12)
                       .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                       .ToObject<Dictionary<string, object>>())
                       .ToList();

                        var table14Data = ((IEnumerable<dynamic>)temp.Table13)
                       .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                       .ToObject<Dictionary<string, object>>())
                       .ToList();

                        var table15Data = ((IEnumerable<dynamic>)temp.Table14)
                       .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                       .ToObject<Dictionary<string, object>>())
                       .ToList();

                        var table16Data = ((IEnumerable<dynamic>)temp.Table15)
                       .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                       .ToObject<Dictionary<string, object>>())
                       .ToList();

                        var table17Data = ((IEnumerable<dynamic>)temp.Table16)
                         .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                         .ToObject<Dictionary<string, object>>())
                         .ToList();

                        var table18Data = ((IEnumerable<dynamic>)temp.Table17)
                        .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                        .ToObject<Dictionary<string, object>>())
                        .ToList();

                        var table19Data = ((IEnumerable<dynamic>)temp.Table18)
                       .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                       .ToObject<Dictionary<string, object>>())
                       .ToList();

                        var table20Data = ((IEnumerable<dynamic>)temp.Table19)
                      .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                      .ToObject<Dictionary<string, object>>())
                      .ToList();

                      var table21Data = ((IEnumerable<dynamic>)temp.Table20)
                     .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                     .ToObject<Dictionary<string, object>>())
                     .ToList();

                    var table22Data = ((IEnumerable<dynamic>)temp.Table21)
                    .Select(row => ((Newtonsoft.Json.Linq.JObject)row)
                    .ToObject<Dictionary<string, object>>())
                    .ToList();

                        var table23Data = ((IEnumerable<dynamic>)temp.Table22)
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
                                table12 = table12Data,
                                table13 = table13Data,
                                table14 = table14Data,
                                table15 = table15Data,
                                table16 = table16Data,
                                table17 = table17Data,
                                table18 = table18Data,
                                table19 = table19Data,
                                table20 = table20Data,
                                table21 = table21Data,
                                table22 = table22Data,
                                table23 = table23Data,
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


        private void GenerateWord(MomRequest data, string wordPath)
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
            var ValidAppln = data.validAppln;
            var AllotmentSummary = data.allotmentSummary;
            var BankMaster = data.bankMaster;
            var CategoryQIB = data.categoryQIB;
            var CategoryMM = data.categoryMM;
            var CategoryEMP = data.categoryEMP;
            var CategoryNIIC = data.categoryNIIC;
            var CategorySOA = data.categorySOA;
            var CategoryEXMMSOA = data.categoryEXMMSOA;
            var CategoryMARMAK = data.categoryMARMAK;
            var CategoryTechRej = data.categoryTechRej;
            var categoryUPISummary = data.categoryUPISummary;

            string clientName = summary.client_name;
            long offer_issuesize = summary.offer_issuesize;
            int offer_facevalue = summary.offer_facevalue;
            int offer_premiun = summary.offer_premiun;
            int offer_fixedprice = summary.offer_fixedprice;
            long total_iposize = summary.total_iposize;
            long mm_shares = summary.mm_shares;
            long total_mm = summary.total_mm;
            long public_shares = summary.public_shares;
            long net_issue = summary.net_issue;
            string offer_openingdate = summary.offer_openingdate;
            string offer_closingdate = summary.offer_closingdate;
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
            long upisum_total_bids = categoryUPISummary.upisum_total_bids;
            long upisum_total_shares = categoryUPISummary.upisum_total_shares;
            long appl_blocked_bids = categoryUPISummary.appl_blocked_bids;
            long appl_blocked_amount = categoryUPISummary.appl_blocked_amount;
            long bid_reg_not_bank_bids = categoryUPISummary.bid_reg_not_bank_bids;
            long bid_reg_not_bank_amount = categoryUPISummary.bid_reg_not_bank_amount;
            long unique_appln = categoryUPISummary.unique_appln;

            string templatePath = _configuration["Appsettings:templatePath"];

            System.IO.File.Copy(templatePath, wordPath, true);

            using (WordprocessingDocument doc = WordprocessingDocument.Open(wordPath, true))
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
                DateTime openingDate = Convert.ToDateTime(summary.offer_openingdate);
                ReplaceText(body, "{offer_openingdate}", openingDate.ToString("dd MMMM yyyy"));
                DateTime closingDate = Convert.ToDateTime(summary.offer_closingdate);
                ReplaceText(body, "{offer_closingdate}", closingDate.ToString("dd MMMM yyyy"));
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
                ReplaceText(body, "{upisum_total_bids}", upisum_total_bids.ToString("N0"));
                ReplaceText(body, "{upisum_total_shares}", upisum_total_shares.ToString("N0"));
                ReplaceText(body, "{appl_blocked_bids}", appl_blocked_bids.ToString("N0"));
                ReplaceText(body, "{appl_blocked_amount}", appl_blocked_amount.ToString("N0"));
                ReplaceText(body, "{bid_reg_not_bank_bids}", bid_reg_not_bank_bids.ToString("N0"));
                ReplaceText(body, "{bid_reg_not_bank_amount}", bid_reg_not_bank_amount.ToString("N0"));
                ReplaceText(body, "{unique_appln}", unique_appln.ToString("N0"));

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
                                CreateCell((bank.bank_name ?? "").ToUpper(), true, false),
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
                                CreateCell((rejection.rejected_reason ?? "").ToUpper(), true, false),
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
                                CreateCell(catInd.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                 CreateCell(catInd.times_subs?.ToString("N4", new CultureInfo("en-IN")), true, true)
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
                                CreateCell(catCo.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(catCo.times_subs?.ToString("N4", new CultureInfo("en-IN")), true, true)

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
                                CreateCell(categoryNRA10L.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRA10L.times_subs?.ToString("N4", new CultureInfo("en-IN")), true, true)
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
                                CreateCell(categoryNRB10L.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNRB10L.times_subs?.ToString("N4", new CultureInfo("en-IN")), true, true)
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

                var para8 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
   .FirstOrDefault(p => p.InnerText.Contains("Summary of Valid Applications"));

                if (para8 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table validAplnTable = null;

                    var next = para8.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            validAplnTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (validAplnTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = validAplnTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 2; i--)
                        {
                            validAplnTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var Appln in ValidAppln)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(Appln.ipo_category.ToUpper(), true, false),
                                CreateCell(Appln.gross_appln.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(Appln.gross_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(Appln.valid_appln.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(Appln.valid_shares.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(Appln.rejected_appln.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(Appln.rejected_shares.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            validAplnTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(ValidAppln.Sum(x => x.gross_appln).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(ValidAppln.Sum(x => x.gross_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(ValidAppln.Sum(x => x.valid_appln).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(ValidAppln.Sum(x => x.valid_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(ValidAppln.Sum(x => x.rejected_appln).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(ValidAppln.Sum(x => x.rejected_shares).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        validAplnTable.Append(totalRow);
                    }
                }

                var para9 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
   .FirstOrDefault(p => p.InnerText.Contains("SUMMARY OF ALLOTMENT OF VARIOUS CATEGORIES AS PER THE ISSUE IS AS UNDER"));

                if (para9 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table summaryAllotTable = null;

                    var next = para9.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            summaryAllotTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (summaryAllotTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = summaryAllotTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 2; i--)
                        {
                            summaryAllotTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var allotSummary in AllotmentSummary)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(allotSummary.ipo_category.ToUpper(), true, false),
                                CreateCell(allotSummary.gross_appln.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.gross_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.valid_appln.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.valid_shares.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.rejected_appln.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.rejected_shares.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.allotment_appln.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(allotSummary.allotment_shares.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            summaryAllotTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(AllotmentSummary.Sum(x => x.gross_appln).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.gross_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.valid_appln).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.valid_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.rejected_appln).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.rejected_shares).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.allotment_appln).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(AllotmentSummary.Sum(x => x.allotment_shares).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        summaryAllotTable.Append(totalRow);
                    }
                }

                var para10 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
      .FirstOrDefault(p => p.InnerText.Contains("Certified Syndicate Banks (SCSBs) for collection of Applications under ASBA Process."));

                if (para10 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table oldTable = null;

                    var next = para10.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            oldTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (oldTable != null)
                    {
                        var parent = oldTable.Parent;

                        // ❌ Remove old broken table
                        parent.RemoveChild(oldTable);

                        // ✅ Create NEW clean table
                        var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                        // Table properties
                        table.AppendChild(
                            new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                                new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                                    new DocumentFormat.OpenXml.Wordprocessing.TopBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.BottomBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.LeftBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.RightBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                                ),
                                new DocumentFormat.OpenXml.Wordprocessing.TableLayout()
                                {
                                    Type = DocumentFormat.OpenXml.Wordprocessing.TableLayoutValues.Fixed
                                }
                            )
                        );

                        // ✅ Define 4 columns (VERY IMPORTANT)
                        table.AppendChild(
                            new DocumentFormat.OpenXml.Wordprocessing.TableGrid(
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "1000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "4000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "1000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "4000" }
                            )
                        );

                        // ✅ HEADER ROW
                        var header = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        header.Append(
                            CreateCell("Sr No", true, false),
                            CreateCell("Bank Name", true, false),
                            CreateCell("Sr No", true, false),
                            CreateCell("Bank Name", true, false)
                        );
                        table.Append(header);

                        // ✅ DATA
                        var bankMasterList = BankMaster ?? new List<BankMaster>();

                        int total = bankMasterList.Count;
                        int half = (int)Math.Ceiling(total / 2.0);

                        int leftSerial = 1;
                        int rightSerial = half + 1;

                        for (int i = 0; i < half; i++)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            string leftSr = "";
                            string leftName = "";
                            string rightSr = "";
                            string rightName = "";

                            if (i < total)
                            {
                                leftSr = leftSerial.ToString();
                                leftName = bankMasterList[i].bank_name?.ToUpper() ?? "";
                                leftSerial++;
                            }

                            if (i + half < total)
                            {
                                rightSr = rightSerial.ToString();
                                rightName = bankMasterList[i + half].bank_name?.ToUpper() ?? "";
                                rightSerial++;
                            }

                            row.Append(
                                CreateCell(leftSr, false, false),
                                CreateCell(leftName, false, false),
                                CreateCell(rightSr, false, false),
                                CreateCell(rightName, false, false)
                            );

                            table.Append(row);
                        }

                        // ✅ Insert new table after paragraph
                        var spacerPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text(" ")
                            )
                        );

                        para10.InsertAfterSelf(spacerPara);
                        spacerPara.InsertAfterSelf(table);
                    }
                }


                var para11 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
      .FirstOrDefault(p => p.InnerText.Contains("of Applications under Syndicate ASBA process. The list is as mentioned below:"));

                if (para11 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table oldTable = null;

                    var next = para11.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            oldTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (oldTable != null)
                    {
                        var parent = oldTable.Parent;

                        // ❌ Remove old broken table
                        parent.RemoveChild(oldTable);

                        // ✅ Create NEW clean table
                        var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                        // Table properties
                        table.AppendChild(
                            new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                                new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                                    new DocumentFormat.OpenXml.Wordprocessing.TopBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.BottomBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.LeftBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.RightBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                                ),
                                new DocumentFormat.OpenXml.Wordprocessing.TableLayout()
                                {
                                    Type = DocumentFormat.OpenXml.Wordprocessing.TableLayoutValues.Fixed
                                }
                            )
                        );

                        // ✅ Define 4 columns (VERY IMPORTANT)
                        table.AppendChild(
                            new DocumentFormat.OpenXml.Wordprocessing.TableGrid(
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "1000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "4000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "1000" },
                                new DocumentFormat.OpenXml.Wordprocessing.GridColumn() { Width = "4000" }
                            )
                        );

                        // ✅ HEADER ROW
                        var header = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        header.Append(
                            CreateCell("Sr No", true, false),
                            CreateCell("Bank Name", true, false),
                            CreateCell("Sr No", true, false),
                            CreateCell("Bank Name", true, false)
                        );
                        table.Append(header);

                        // ✅ DATA
                        var bankMasterList = BankMaster ?? new List<BankMaster>();

                        int total = bankMasterList.Count;
                        int half = (int)Math.Ceiling(total / 2.0);

                        int leftSerial = 1;
                        int rightSerial = half + 1;

                        for (int i = 0; i < half; i++)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            string leftSr = "";
                            string leftName = "";
                            string rightSr = "";
                            string rightName = "";

                            if (i < total)
                            {
                                leftSr = leftSerial.ToString();
                                leftName = bankMasterList[i].bank_name?.ToUpper() ?? "";
                                leftSerial++;
                            }

                            if (i + half < total)
                            {
                                rightSr = rightSerial.ToString();
                                rightName = bankMasterList[i + half].bank_name?.ToUpper() ?? "";
                                rightSerial++;
                            }

                            row.Append(
                                CreateCell(leftSr, false, false),
                                CreateCell(leftName, false, false),
                                CreateCell(rightSr, false, false),
                                CreateCell(rightName, false, false)
                            );

                            table.Append(row);
                        }

                        // ✅ Insert new table after paragraph
                        var spacerPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text(" ")
                            )
                        );

                        para11.InsertAfterSelf(spacerPara);
                        spacerPara.InsertAfterSelf(table);
                    }
                }

                var para12 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("Net Collections of QIBs Category Applications by ASBA:"));

                if (para12 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catQIBTable = null;

                    var next = para12.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catQIBTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catQIBTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catQIBTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catQIBTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryQIB in CategoryQIB)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(categoryQIB.bank_name.ToUpper(), true, false),
                                CreateCell(categoryQIB.no_of_applications.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryQIB.no_of_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryQIB.total_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            catQIBTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategoryQIB.Sum(x => x.no_of_applications).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryQIB.Sum(x => x.no_of_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryQIB.Sum(x => x.total_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        catQIBTable.Append(totalRow);
                    }
                }

                var para13 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("Net Collections of Market Makers Category Applications by ASBA:"));

                if (para13 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catMMTable = null;

                    var next = para13.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catMMTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catMMTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catMMTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catMMTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryMM in CategoryMM)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(categoryMM.bank_name.ToUpper(), true, false),
                                CreateCell(categoryMM.no_of_applications.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryMM.no_of_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryMM.total_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            catMMTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategoryMM.Sum(x => x.no_of_applications).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryMM.Sum(x => x.no_of_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryMM.Sum(x => x.total_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        catMMTable.Append(totalRow);
                    }
                }

                var para14 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("EMPLOYEES"));

                if (para14 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catEMPTable = null;

                    var next = para14.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catEMPTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catEMPTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catEMPTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catEMPTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryEMP in CategoryEMP)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(categoryEMP.bank_name.ToUpper(), true, false),
                                CreateCell(categoryEMP.no_of_applications.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryEMP.no_of_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryEMP.total_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            catEMPTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategoryEMP.Sum(x => x.no_of_applications).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryEMP.Sum(x => x.no_of_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryEMP.Sum(x => x.total_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        catEMPTable.Append(totalRow);
                    }
                }

                var para15 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
  .FirstOrDefault(p => p.InnerText.Contains("Public Category (Non-Institutional and Individual Investor Category)"));

                if (para15 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table NIICTable = null;

                    var next = para15.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            NIICTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (NIICTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = NIICTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 2; i--)
                        {
                            NIICTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryNIIC in CategoryNIIC)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(srNo.ToString(), true, false),
                                CreateCell(categoryNIIC.Particulars.ToUpper(), true, false),
                                CreateCell(categoryNIIC.nii_no_of_applications.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNIIC.nii_no_of_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNIIC.ind_no_of_applications.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNIIC.ind_no_of_shares.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNIIC.total_no_of_applications.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryNIIC.total_no_of_shares.ToString("N2", new CultureInfo("en-IN")), true, true)
                            );

                            NIICTable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("", true, false),
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategoryNIIC.Sum(x => x.nii_no_of_applications).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryNIIC.Sum(x => x.nii_no_of_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryNIIC.Sum(x => x.ind_no_of_applications).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryNIIC.Sum(x => x.ind_no_of_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryNIIC.Sum(x => x.total_no_of_applications).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryNIIC.Sum(x => x.total_no_of_shares).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        NIICTable.Append(totalRow);
                    }
                }

                var para16 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("SUMMARY (EXCLUDING ANCHOR PORTION)"));

                if (para16 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catSOATable = null;

                    var next = para16.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catSOATable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catSOATable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catSOATable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catSOATable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categorySOA in CategorySOA)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categorySOA.ipo_category.ToUpper(), true, false),
                                CreateCell(categorySOA.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.valid_shares_received.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.equity_shares_allotted.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.total_allotment_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            catSOATable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategorySOA.Sum(x => x.offer_cat_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.valid_shares_received).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.equity_shares_allotted).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.total_allotment_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        catSOATable.Append(totalRow);
                    }
                    var qibData = CategorySOA
                        .FirstOrDefault(x => x.ipo_category != null
                                          && x.ipo_category.ToUpper().Contains("QIB"));

                                        string qibOfferShares = qibData != null
                                            ? qibData.offer_cat_shares.ToString("N0", new CultureInfo("en-IN"))
                                            : "0";
                                        string qibNoOfShares = qibData != null
                                        ? qibData.valid_shares_received.ToString("N0", new CultureInfo("en-IN"))
                                        : "0";
                    ReplaceText(body, "{offer_cat_shares}", qibOfferShares);
                    ReplaceText(body, "{qib_no_of_shares}", qibNoOfShares);
                }

                var para17 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("SUMMARY (INCLUDING ANCHOR PORTION)"));

                if (para17 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catSOATable = null;

                    var next = para17.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catSOATable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catSOATable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catSOATable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catSOATable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categorySOA in CategorySOA)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categorySOA.ipo_category.ToUpper(), true, false),
                                CreateCell(categorySOA.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.valid_shares_received.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.equity_shares_allotted.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categorySOA.total_allotment_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            catSOATable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategorySOA.Sum(x => x.offer_cat_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.valid_shares_received).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.equity_shares_allotted).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategorySOA.Sum(x => x.total_allotment_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        catSOATable.Append(totalRow);
                    }
                }

                var para18 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("SUMMARY (EXCLUDING ANCHOR PORTION)"));

                if (para18 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table categoryEXMMSOATable = null;

                    var next = para18.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            categoryEXMMSOATable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (categoryEXMMSOATable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = categoryEXMMSOATable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            categoryEXMMSOATable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryEXMMSOA in CategoryEXMMSOA)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categoryEXMMSOA.ipo_category.ToUpper(), true, false),
                                CreateCell(categoryEXMMSOA.offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryEXMMSOA.valid_shares_received.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryEXMMSOA.equity_shares_allotted.ToString("N2", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryEXMMSOA.total_allotment_amount.ToString("N2", new CultureInfo("en-IN")), true, true)

                            );

                            categoryEXMMSOATable.Append(row);
                            srNo++;
                        }

                        // ✅ TOTAL ROW
                        var totalRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        totalRow.Append(
                            CreateCell("TOTAL", true, false),
                            CreateCell(CategoryEXMMSOA.Sum(x => x.offer_cat_shares).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryEXMMSOA.Sum(x => x.valid_shares_received).ToString("N0", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryEXMMSOA.Sum(x => x.equity_shares_allotted).ToString("N2", new CultureInfo("en-IN")), true, true),
                            CreateCell(CategoryEXMMSOA.Sum(x => x.total_allotment_amount).ToString("N2", new CultureInfo("en-IN")), true, true)
                        );

                        categoryEXMMSOATable.Append(totalRow);
                    }
                }

                var para19 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("Equity Shares reserved for this category resulting in subscription"));

                if (para19 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catMARMAKTable = null;

                    var next = para19.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catMARMAKTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catMARMAKTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catMARMAKTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ToList();

                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catMARMAKTable.RemoveChild(rows[i]);
                        }

                        int srNo = 1;

                        foreach (var categoryMARMAK in CategoryMARMAK)
                        {
                            var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                            row.Append(
                                CreateCell(categoryMARMAK.mm_total_appl.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryMARMAK.mm_total_quantity.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryMARMAK.mm_offer_cat_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(categoryMARMAK.mm_times_subs?.ToString("N4", new CultureInfo("en-IN")), true, true)
                            );

                            catMARMAKTable.Append(row);
                            srNo++;
                        }

                    }

                    var item = CategoryMARMAK.FirstOrDefault();

                    ReplaceText(body, "{mm_total_appl}", item.mm_total_appl.ToString("N0", new CultureInfo("en-IN")));
                    ReplaceText(body, "{mm_total_quantity}", item.mm_total_quantity.ToString("N0", new CultureInfo("en-IN")));
                    ReplaceText(body, "{mm_offer_cat_shares}", item.mm_offer_cat_shares.ToString("N0", new CultureInfo("en-IN")));
                    ReplaceText(body, "{mm_times_subs}", item.mm_times_subs?.ToString("N2", new CultureInfo("en-IN")) ?? "0.00");
                }

                var para20 = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
.FirstOrDefault(p => p.InnerText.Contains("to be considered for allotment are as per the table given below."));

                if (para20 != null)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table catTechRejTable = null;

                    var next = para20.NextSibling();

                    while (next != null)
                    {
                        if (next is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                        {
                            catTechRejTable = tbl;
                            break;
                        }
                        next = next.NextSibling();
                    }

                    if (catTechRejTable != null)
                    {
                        // ✅ Remove old rows except header
                        var rows = catTechRejTable.Elements<TableRow>().ToList();

                        // Remove old rows except header
                        for (int i = rows.Count - 1; i > 0; i--)
                        {
                            catTechRejTable.RemoveChild(rows[i]);
                        }

                        var item = CategoryTechRej.FirstOrDefault();

                        if (item != null)
                        {
                            // Row 1 - Total Bids
                            catTechRejTable.Append(new TableRow(
                                CreateCell("1", true, false),
                                CreateCell("Total Bids", true, false),
                                CreateCell(item.total_appls.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.total_appl_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.total_appl_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            ));

                            // Row 2 - Technical Rejections
                            catTechRejTable.Append(new TableRow(
                                CreateCell("2", true, false),
                                CreateCell("Technical Rejections", true, false),
                                CreateCell(item.rejected_appls.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.rejected_appl_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.rejected_appl_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            ));

                            // Row 3 - Net Quantum
                            catTechRejTable.Append(new TableRow(
                                CreateCell("3", true, false),
                                CreateCell("Net Quantum eligible for allotment", true, false),
                                CreateCell(item.net_appls.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.net_appl_shares.ToString("N0", new CultureInfo("en-IN")), true, true),
                                CreateCell(item.net_appl_amount.ToString("N2", new CultureInfo("en-IN")), true, true)
                            ));
                        }

                        // 🔥 NEXT STEP: Bank Table (you will add here)

                        doc.MainDocumentPart.Document.Save();
                    }
                }
            }
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


        [HttpPost]
        public IActionResult DownloadAllReports([FromBody] MomRequest data)
        {
            string folder = Path.Combine(Path.GetTempPath(), "IPOReports");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string uniqueId = DateTime.Now.Ticks.ToString();

            string excelPath = Path.Combine(folder, $"BOA_{uniqueId}.xlsx");
            string wordPath = Path.Combine(folder, $"MOM_{uniqueId}.docx");
            string zipPath = Path.Combine(folder, $"IPO_{uniqueId}.zip");

            try
            {
                // Generate files
                GenerateExcel(data.summary.offer_code, excelPath);
                GenerateWord(data, wordPath);

                // Create ZIP
                using (var zipStream = new FileStream(zipPath, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(excelPath, "BOA_Report.xlsx");
                    archive.CreateEntryFromFile(wordPath, "MOM_Report.docx");
                }

                // Return file as stream (better than ReadAllBytes)
                var stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);

                return File(stream, "application/zip", "IPO_Reports.zip");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            finally
            {
                // ✅ Cleanup (VERY IMPORTANT)
                TryDelete(excelPath);
                TryDelete(wordPath);
                //TryDelete(zipPath);
            }
        }

        private void TryDelete(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch { /* ignore */ }
        }

        [HttpGet]
        public IActionResult Export_allotment_bo(string offer_code)
        {
            string urlstring = Convert.ToString(
                _configuration.GetSection("Appsettings")["apiurl"]) + "Export_allotment_bo";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    string APIcookieName =
                        "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[APIcookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = client.GetAsync(urlstring + "?offer_code=" + offer_code).Result;

                    if (!response.IsSuccessStatusCode)
                        return Problem("API call failed");

                    string resultMessage = response.Content.ReadAsStringAsync().Result;

                    var ds = JsonConvert.DeserializeObject<DataSet>(resultMessage);

                    var stream = new MemoryStream();

                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                    {
                        // =========================
                        // 📄 FILE 1 → CDSL
                        // =========================
                        var entry1 = archive.CreateEntry("ipo_output_CDSL.txt");

                        using (var writer = new StreamWriter(entry1.Open()))
                        {
                            writer.WriteLine(
                                "DP-ID".PadRight(10) +
                                "CLNT-ID".PadRight(10) +
                                "ALLOTED QUANTITY".PadRight(150)
                            );

                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                writer.WriteLine(
                                    (row["dp_id"]?.ToString() ?? "").PadRight(10) +
                                    (row["client_id"]?.ToString() ?? "").PadRight(10) +
                                    (row["alloted_quantity"]?.ToString() ?? "").PadRight(150)
                                );
                            }
                        }

                        // =========================
                        // 📄 FILE 2 → NSDL
                        // =========================
                        var entry2 = archive.CreateEntry("ipo_output_NSDL.txt");

                        using (var writer = new StreamWriter(entry2.Open()))
                        {
                            writer.WriteLine(
                                "DP-ID".PadRight(10) +
                                "CLNT-ID".PadRight(10) +
                                "ALLOTED QUANTITY".PadRight(150)
                            );

                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                writer.WriteLine(
                                    (row["dp_id"]?.ToString() ?? "").PadRight(10) +
                                    (row["client_id"]?.ToString() ?? "").PadRight(10) +
                                    (row["alloted_quantity"]?.ToString() ?? "").PadRight(150)
                                );
                            }
                        }
                    }

                    stream.Position = 0;

                    return File(stream, "application/zip", "ipo_allotment.zip");
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}

