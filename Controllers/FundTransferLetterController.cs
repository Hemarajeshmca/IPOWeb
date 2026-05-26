using DocumentFormat.OpenXml.Presentation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using PdfDocument = iTextSharp.text.Document;
using PdfFont = iTextSharp.text.Font;
using PdfFontFactory = iTextSharp.text.FontFactory;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;

namespace IPOWeb.Controllers
{
    public class FundTransferLetterController : Controller
    {
        public IActionResult FundTransferLetter()
        {
            return View();
        }     

        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public FundTransferLetterController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }
        string urlstring = "";
        string APIcookieName = "";

        [HttpGet]
        public IActionResult getBankFundDetails(string offer_code)
        {
            try
            {
                string urlstring = _configuration["Appsettings:apiurl"] + "getBankFundDetails";

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
        public IActionResult DownloadAllZip(string offer_code, string curdate,string trandate,string bank_name,string banker_address, string account_no, string ifsc,string account_title)
        {
            try
            {
                // =========================
                // ✅ TEMP FOLDER
                // =========================
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                var pdfFolder = Path.Combine(tempFolder, "PDFs");
                var txtFolder = Path.Combine(tempFolder, "BankDetails");

                Directory.CreateDirectory(pdfFolder);
                Directory.CreateDirectory(txtFolder);

                // =========================
                // ✅ FORMAT DATES
                // =========================
                string formattedCurDate = curdate;

                string formattedTranDate = "";
                if (!string.IsNullOrEmpty(trandate))
                {
                    DateTime dt = DateTime.Parse(trandate);
                    formattedTranDate = dt.ToString("dd MMMM yyyy");
                }

                // =========================
                // ✅ CALL BANK API
                // =========================
                string urlstring = _configuration["Appsettings:apiurl"] + "getBankFundDetails";

                List<NSBBankData> nsbbankList;
                List<SBBankData> sbbankList;

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

                    nsbbankList = apiData?.nsbsummary ?? new List<NSBBankData>();
                    sbbankList = apiData?.sbsummary ?? new List<SBBankData>();
                }

                // =========================
                // ✅ PDF GENERATION
                // =========================            

                var allBanks = nsbbankList
                    .Select(x => x.bank_name?.Trim().ToUpper())
                    .Union(
                        sbbankList.Select(x => x.bank_name?.Trim().ToUpper())
                    )
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                foreach (var bankName in allBanks)
                {
                    // 🔍 find matching NSB bank
                    // NSB record
                    var nsb = nsbbankList
                        .FirstOrDefault(x =>
                            x.bank_name?.Trim().ToUpper() == bankName);

                    // SB record
                    var sb = sbbankList
                        .FirstOrDefault(x =>
                            x.bank_name?.Trim().ToUpper() == bankName);


                    string safeName = string.Join("_",
                         bankName.Split(Path.GetInvalidFileNameChars()));

                    string pdfPath = Path.Combine(pdfFolder, $"{safeName}.pdf");

                    using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                    {
                        var doc = new iTextSharp.text.Document(
                            iTextSharp.text.PageSize.A4, 40, 40, 40, 40);

                        PdfWriter.GetInstance(doc, fs);
                        doc.Open();

                        // ===== HEADER =====
                        PdfPTable headerTable = new PdfPTable(3);
                        headerTable.WidthPercentage = 100;
                        headerTable.SetWidths(new float[] { 20f, 60f, 20f });

                        string logoPath = Path.Combine(_env.WebRootPath, "assets", "images", "logognsaupdated.jpg");
                        var logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(70f, 70f);

                        PdfPCell logoCell = new PdfPCell(logo)
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        headerTable.AddCell(logoCell);

                        Paragraph address = new Paragraph
                        {
                            Alignment = Element.ALIGN_CENTER,
                            Font = FontFactory.GetFont(FontFactory.HELVETICA, 10)
                        };

                        var companyboldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                        address.Add(new Chunk("GNSA Infotech (P) Ltd. \n", companyboldFont));
                        address.Add("Category II Share Transfer Agent Registration No. INR200003967\n");
                        address.Add("CIN: U65993TN1994PTC027878\n\n");

                        address.Add(new Chunk("Registered address of Branch Office : \n", companyboldFont));
                        address.Add("4th and 5th Floors, F-Block, Nelson Chambers\n");
                        address.Add("No.115, Nelson Manickam Road, Aminjikarai, Chennai 600030\n");
                        address.Add("Tel : +91-44-4296 2025, Email: sta@gnsaindia.com\n");

                        PdfPCell addressCell = new PdfPCell(address)
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        headerTable.AddCell(addressCell);

                        headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });

                        doc.Add(headerTable);
                        doc.Add(new Paragraph("\n"));

                        // DATE
                        doc.Add(new Paragraph(formattedCurDate, normalFont));
                        doc.Add(new Paragraph("\n"));

                        // HEADER
                        doc.Add(new Paragraph("The Manager,", companyboldFont));
                        //doc.Add(new Paragraph(bankName, companyboldFont));
                        doc.Add(new Paragraph(
                                sb?.bank_name ?? nsb?.bank_name ?? bankName,
                                companyboldFont));
                        doc.Add(new Paragraph("\n"));

                        //doc.Add(new Paragraph($"Sub : SME PUBLIC ISSUE OF {bank.client_name}", companyboldFont));
                        doc.Add(new Paragraph(
                            $"Sub : SME PUBLIC ISSUE OF {sb?.client_name ?? nsb?.client_name}",
                            companyboldFont));
                        doc.Add(new Paragraph("\n"));

                        doc.Add(new Paragraph("Dear Sir,", normalFont));
                        doc.Add(new Paragraph(
                            "We hereby instruct you to transfer the amount adjusted towards shares allotted pertaining to your ASBA / Non ASBA application as per the data attached with contained the details of the investors from whose accounts the money should be transferred to the Public Issue Account.",
                            normalFont));

                        doc.Add(new Paragraph("\n"));

                        // ===== TABLE 1 =====
                        PdfPTable table = new PdfPTable(4);
                        table.WidthPercentage = 100;

                        AddCell(table, "Member Type", companyboldFont);
                        AddCell(table, "Blocked Amount (Rs.)", companyboldFont);
                        AddCell(table, "Transfer Amount (Rs.)", companyboldFont);
                        AddCell(table, "Unblocked Amount (Rs.)", companyboldFont);

                        AddCell(table, "NSM", normalFont);
                        AddCell(table,
                         nsb != null ? nsb.nsb_allocated_block_amount.ToString("N2") : "0.00",
                         normalFont, Element.ALIGN_RIGHT);

                        AddCell(table,
                            nsb != null ? nsb.nsb_total_amount.ToString("N2") : "0.00",
                            normalFont, Element.ALIGN_RIGHT);

                    AddCell(table,
                        nsb != null ? nsb.nsb_unblocked_amount.ToString("N2") : "0.00",
                        normalFont, Element.ALIGN_RIGHT);

                        AddCell(table, "SM", normalFont);
                        //AddCell(table, bank.sb_allocated_block_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);
                        //AddCell(table, bank.sb_total_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);
                        //AddCell(table, bank.sb_unblocked_amount.ToString("N2"), normalFont, Element.ALIGN_RIGHT);

                        AddCell(table,
                            sb != null ? sb.sb_total_amount.ToString("N2") : "0.00",
                            normalFont,
                            Element.ALIGN_RIGHT);

                        AddCell(table,
                            sb != null ? sb.sb_allocated_block_amount.ToString("N2") : "0.00",
                            normalFont,
                            Element.ALIGN_RIGHT);

                        AddCell(table,
                            sb != null ? sb.sb_unblocked_amount.ToString("N2") : "0.00",
                            normalFont,
                            Element.ALIGN_RIGHT);

                        AddCell(table, "Total", boldFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);
                        AddCell(table, "", normalFont);


                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));

                        // ===== ACCOUNT DETAILS =====
                        doc.Add(new Paragraph("The detail of the Public Issue Account is below :", normalFont));
                        doc.Add(new Paragraph("\n"));

                        doc.Add(new Paragraph($"Bank Name : {bank_name}", normalFont));
                        doc.Add(new Paragraph($"Branch Name : {banker_address}", normalFont));
                        doc.Add(new Paragraph("\n"));

                        // ===== TABLE 2 =====
                        PdfPTable table1 = new PdfPTable(4);
                        table1.WidthPercentage = 100;

                        AddCell(table1, "Bank Name", boldFont);
                        AddCell(table1, "Account No.", boldFont);
                        AddCell(table1, "IFSC / RTGS / NEFT", boldFont);
                        AddCell(table1, "Account Title", boldFont);

                        AddCell(table1, bank_name, normalFont);
                        AddCell(table1, account_no, normalFont);
                        AddCell(table1, ifsc, normalFont);
                        AddCell(table1, account_title, normalFont);
                        doc.Add(table1);

                        doc.Add(new Paragraph("\n"));
                        doc.Add(new Paragraph($"We request you to transfer the above funds immediately on {formattedTranDate}", normalFont));
                        doc.Add(new Paragraph("Your cooperation in this regard is highly appreciated", normalFont));
                        doc.Add(new Paragraph("Thanking You", normalFont));

                        doc.Close();
                    }
                }

                // =========================
                // ✅ ALLOTMENT FILES
                // =========================
                string urlstring1 =
     _configuration["Appsettings:apiurl"] +
     "Fund_Transfer_bank_details";

                using (var client = new HttpClient())
                {
                    string token = Request.Cookies["APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var response =
                        client.GetAsync(urlstring1 + "?offer_code=" + offer_code).Result;

                    if (!response.IsSuccessStatusCode)
                        return BadRequest("Excel API failed");

                    // ✅ READ JSON
                    string result =
                        response.Content.ReadAsStringAsync().Result;

                    // ✅ DESERIALIZE
                    var excelFiles =
                        JsonConvert.DeserializeObject<List<ExcelFileModel>>(result);

                    // ✅ SAVE EXCEL FILES
                    foreach (var file in excelFiles)
                    {
                        string filePath =
                            Path.Combine(txtFolder, file.FileName);

                        System.IO.File.WriteAllBytes(
                            filePath,
                            file.Content);
                    }
                }

                // =========================
                // ✅ FINAL ZIP
                // =========================
                using (var memoryStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
                        {
                            string entryName = file.Replace(tempFolder + Path.DirectorySeparatorChar, "");

                            var entry = zip.CreateEntry(entryName);

                            using (var entryStream = entry.Open())
                            using (var fs = new FileStream(file, FileMode.Open))
                            {
                                fs.CopyTo(entryStream);
                            }
                        }
                    }

                    Directory.Delete(tempFolder, true);

                    return File(memoryStream.ToArray(), "application/zip", "Final_Output.zip");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void AddCell(PdfPTable table, string text, iTextSharp.text.Font font, int align = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = align;
            table.AddCell(cell);
        }

        public class ExcelFileModel
        {
            public string FileName { get; set; }

            public byte[] Content { get; set; }
        }

        public class NSBBankData
        {
            public string bank_code { get; set; }
            public string bank_name { get; set; }
            public string client_name { get; set; }
            public long nsb_total_amount { get; set; }
            public long nsb_allocated_block_amount { get; set; }
            public long nsb_unblocked_amount { get; set; }
        }

        public class SBBankData
        {
            public string bank_code { get; set; }
            public string bank_name { get; set; }
            public string client_name { get; set; }
            public long sb_total_amount { get; set; }
            public long sb_allocated_block_amount { get; set; }
            public long sb_unblocked_amount { get; set; }
        }


        public class BankerData
        {
            public string bank_code { get; set; }
            public string bank_name { get; set; }
            public string bank_type { get; set; }
            public string banker_address { get; set; }
            public string banker_accountno { get; set; }
            public string banker_ifsc { get; set; }
        }

        public class BankApiResponse
        {
            public List<NSBBankData> nsbsummary { get; set; }
            public List<SBBankData> sbsummary { get; set; }
            public List<BankerData> banker { get; set; }
        }
    }
}
