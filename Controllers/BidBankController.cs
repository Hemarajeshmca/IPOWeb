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
using Word = Microsoft.Office.Interop.Word;
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
        public JsonResult getBidBank(string offer_code)
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


        [HttpGet]
        public IActionResult getBankDetails(string offer_code)
        {
            try
            {
                string urlstring = _configuration["Appsettings:apiurl"] + "getBankDetails";

                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    string cookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[cookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = urlstring + "?offer_code=" + offer_code;

                    var response = client.GetAsync(url).Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(cookieName);
                        return Json(new { success = false, authExpired = true });
                    }

                    if (!response.IsSuccessStatusCode)
                        return Json(new { success = false, message = "API failed" });

                    string result = response.Content.ReadAsStringAsync().Result;

                    return Content(result, "application/json");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DownloadBankZip(string offer_code)
        {
            try
            {
                string urlstring = _configuration["Appsettings:apiurl"] + "getBankDetails";

                List<BankData> bankList;
                List<BankerData> bankerList;

                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    string cookieName = "APItoken-" +
                                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[cookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(urlstring + "?offer_code=" + offer_code).Result;

                    if (!response.IsSuccessStatusCode)
                        return BadRequest("API failed");

                    string result = response.Content.ReadAsStringAsync().Result;

                    var apiData = JsonConvert.DeserializeObject<BankApiResponse>(result);

                    bankList = apiData?.summary ?? new List<BankData>();

                    // IMPORTANT: banker is SINGLE RECORD (not per bank)
                    bankerList = apiData?.banker ?? new List<BankerData>();
                }

                // ✔ Single banker record
                var banker = bankerList.FirstOrDefault();

                string accountNo = banker?.banker_accountno ?? "";
                string ifsc = banker?.banker_ifsc ?? "";
                string bankerAddress = banker?.banker_address ?? "";
                string bankerBankName = banker?.bank_name ?? "";

                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                foreach (var bank in bankList)
                {
                    string safeName = string.Join("_",
                        bank.bank_name.Split(Path.GetInvalidFileNameChars()));

                    string pdfPath = Path.Combine(tempFolder, $"{safeName}.pdf");

                    using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                    {
                        var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 40, 40, 40, 40);
                        PdfWriter.GetInstance(doc, fs);
                        doc.Open();

                        // ===== HEADER (LOGO + ADDRESS) =====
                        PdfPTable headerTable = new PdfPTable(3);
                        headerTable.WidthPercentage = 100;
                        headerTable.SetWidths(new float[] { 20f, 60f, 20f });

                        // LEFT - LOGO
                        string logoPath = Path.Combine(_env.WebRootPath,"assets", "images", "logognsaupdated.jpg");
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(70f, 70f);

                        PdfPCell logoCell = new PdfPCell(logo);
                        logoCell.Border = Rectangle.NO_BORDER;
                        logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        headerTable.AddCell(logoCell);

                        // CENTER - ADDRESS
                        Paragraph address = new Paragraph();
                        address.Alignment = Element.ALIGN_CENTER;
                        address.Font = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        Font companyboldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                       
                        address.Add(new Chunk("GNSA Infotech (P) Ltd. \n", companyboldFont));
                        address.Add("Category II Share Transfer Agent Registration No. INR200003967\n");
                        address.Add("CIN: U65993TN1994PTC027878\n");
                        address.Add("\n");                                        

                        address.Add(new Chunk("Registered address of Branch Office : \n", companyboldFont));
                        address.Add("4th and 5th Floors, F-Block, Nelson Chambers\n");
                        address.Add("No.115, Nelson Manickam Road, Aminjikarai, Chennai 600030\n");
                        address.Add("Tel : +91- 44 – 4296 2025, Email: sta@gnsaindia.com\n");                        

                        PdfPCell addressCell = new PdfPCell(address);
                        addressCell.Border = Rectangle.NO_BORDER;
                        addressCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerTable.AddCell(addressCell);

                        // RIGHT - EMPTY
                        PdfPCell emptyCell = new PdfPCell(new Phrase(""));
                        emptyCell.Border = Rectangle.NO_BORDER;
                        headerTable.AddCell(emptyCell);

                        // ADD HEADER TO DOC
                        doc.Add(headerTable);

                        // SPACE AFTER HEADER
                        doc.Add(new Paragraph("\n"));

                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                        // DATE
                        doc.Add(new Paragraph(DateTime.Now.ToString("dd MMMM yyyy"), normalFont));
                        doc.Add(new Paragraph("\n"));

                        // HEADER
                        doc.Add(new Paragraph("The Manager,", companyboldFont));
                        doc.Add(new Paragraph(bank.bank_name, companyboldFont));
                        doc.Add(new Paragraph("\n"));

                        doc.Add(new Paragraph($"Sub : SME PUBLIC ISSUE OF {bank.client_name}", companyboldFont));
                        doc.Add(new Paragraph("\n"));

                        doc.Add(new Paragraph("Dear Sir,", normalFont));
                        doc.Add(new Paragraph(
                            "We hereby instruct you to transfer the amount adjusted towards shares allotted pertaining to your ASBA / Non ASBA application as per the data attached with contained the details of the investors from whose accounts the money should be transferred to the Public Issue Account.",
                            normalFont));
                        doc.Add(new Paragraph("\n"));

                        // ===== TABLE 1 =====
                        PdfPTable table = new PdfPTable(4);
                        table.WidthPercentage = 100;
                        table.SetWidths(new float[] { 15, 30, 30, 25 });

                        AddCell(table, "Member Type", boldFont);
                        AddCell(table, "Blocked Amount (Rs.)", boldFont);
                        AddCell(table, "Transfer Amount (Rs.)", boldFont);
                        AddCell(table, "Unblocked Amount (Rs.)", boldFont);

                        AddCell(table, "NSM", normalFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);

                        AddCell(table, "SM", normalFont);
                        AddCell(table, bank.allocated_block_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);
                        AddCell(table, bank.total_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);
                        AddCell(table, bank.unblocked_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);

                        AddCell(table, "Total", boldFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);

                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));

                        // ===== ACCOUNT DETAILS =====
                        doc.Add(new Paragraph("The detail of the Public Issue Account is below :", normalFont));
                        doc.Add(new Paragraph("\n"));

                        doc.Add(new Paragraph($"Bank Name : {bankerBankName}", normalFont));
                        doc.Add(new Paragraph($"Branch Name : {bankerAddress}", normalFont));
                        doc.Add(new Paragraph("\n"));

                        // ===== TABLE 2 =====
                        PdfPTable table1 = new PdfPTable(4);
                        table1.WidthPercentage = 100;

                        AddCell(table1, "Bank Name", boldFont);
                        AddCell(table1, "Account No.", boldFont);
                        AddCell(table1, "IFSC / RTGS / NEFT", boldFont);
                        AddCell(table1, "Account Title", boldFont);

                        AddCell(table1, bankerBankName, normalFont);
                        AddCell(table1, accountNo, normalFont);
                        AddCell(table1, ifsc, normalFont);
                        AddCell(table1, bank.client_name, normalFont);

                        doc.Add(table1);

                        doc.Add(new Paragraph("\n"));
                        doc.Add(new Paragraph("We request you to transfer the above funds immediately on ", normalFont));
                        doc.Add(new Paragraph("Your cooperation in this regard is highly appreciated", normalFont));
                        doc.Add(new Paragraph("Thanking You", normalFont));

                        doc.Close();
                    }
                }

                // ===== ZIP CREATION =====
                using (var memoryStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in Directory.GetFiles(tempFolder, "*.pdf"))
                        {
                            var entry = zip.CreateEntry(Path.GetFileName(file));

                            using (var entryStream = entry.Open())
                            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                fs.CopyTo(entryStream);
                            }
                        }
                    }

                    Directory.Delete(tempFolder, true);

                    return File(memoryStream.ToArray(),
                        "application/zip",
                        "BankDocuments.zip");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void AddCell(PdfPTable table, string text, Font font, int alignment = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            table.AddCell(cell);
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
  

    public class BankData
    {
        public string bank_code { get; set; }
        public string bank_name { get; set; }
        public string client_name { get; set; }
        public long total_amount { get; set; }
        public long allocated_block_amount { get; set; }
        public long unblocked_amount { get; set; }
    }


    public class BankerData
    {
        public string bank_code { get; set; }
        public string bank_name { get; set; }        
        public string banker_address { get; set; }
        public string banker_accountno { get; set; }
        public string banker_ifsc { get; set; }       
    }

    public class BankApiResponse
    {
        public List<BankData> summary { get; set; }
        public List<BankerData> banker { get; set; }
    }
}
