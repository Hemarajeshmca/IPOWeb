using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;

namespace IPOWeb.Controllers
{
    public class BidBankController : Controller
    {
        public IActionResult BidBank()
        {
            return View();
        }

        private IConfiguration _configuration;
        public BidBankController(IConfiguration configuration)
        {
            _configuration = configuration;
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


        public IActionResult GetBankList(int? id)
        {
            var data = new List<DatasetModel>
            {
                new DatasetModel { Id = 1, bankName = "ICICI Bank", Status = "Tallied" },
                new DatasetModel { Id = 2, bankName = "HDFC Bank", Status = "Tallied" },
                new DatasetModel { Id = 3, bankName = "Axis Bank",  Status = "Not Tallied"},
                new DatasetModel { Id = 4, bankName = "HBD Finance Groups",  Status = "Tallied"},
                new DatasetModel { Id = 5, bankName = "Canara Bank",  Status = "Not Tallied"},
                new DatasetModel { Id = 6, bankName = "SBI Bank",  Status = "Tallied"},
                new DatasetModel { Id = 7, bankName = "IOB Bank",  Status = "Not Tallied"} ,               
                new DatasetModel { Id = 8, bankName = "TMB Bank",  Status = "Not Tallied"},  
                new DatasetModel { Id = 9, bankName = "KVB Bank",  Status = "Not Tallied"} ,               
                new DatasetModel { Id = 10, bankName = "SC Bank",  Status = "Not Tallied"},                
                new DatasetModel { Id = 11, bankName = "HSBC Bank",  Status = "Not Tallied"}           
                          
            };

            if (id.HasValue)
                data = data.Where(x => x.Id == id.Value).ToList();

            return Json(data);
        }

        public class DatasetModel
        {
            public int Id { get; set; }
            public string bankName { get; set; }
            public string Category { get; set; }
            public string Status { get; set; }
            public string LastSyncDate { get; set; }
            public string LastSyncStatus { get; set; }
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
