using Microsoft.AspNetCore.Mvc;

namespace IPOWeb.Controllers
{
    public class BidBankController : Controller
    {
        public IActionResult BidBank()
        {
            return View();
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
