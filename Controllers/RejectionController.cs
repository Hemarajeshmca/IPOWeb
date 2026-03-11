using Microsoft.AspNetCore.Mvc;

namespace IPOWeb.Controllers
{
    public class RejectionController : Controller
    {
        public IActionResult Rejection()
        {
            return View();
        }

        public IActionResult GetRejectionList(int? id)
        {
            var data = new List<DatasetModel>
            {
                new DatasetModel { Id = 1, bankName = "Pan Mismatch", Status = "Active" },
                new DatasetModel { Id = 2, bankName = "Inactive Account", Status = "Active" },
                new DatasetModel { Id = 3, bankName = "Invalid DPID/Client ID",  Status = "Active"},
                new DatasetModel { Id = 3, bankName = "Multiple Application with Common Pan",  Status = "Active"},
                new DatasetModel { Id = 4, bankName = "Bid by OCB",  Status = "Active"},
                new DatasetModel { Id = 4, bankName = "Bid by OCB - Depository Receipt",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Application by Corporate in Retail Category",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Application not in Electronic Book",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Bid Not Banked",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Application with Insufficient Fund",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Withdrawal of Application",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Duplicate Bid",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Multiple Price Bid",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Bid not in Lot Size",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Difference in Quantity",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Ineligible Shareholers for Call Money",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Mismatch of Application Amount with Shares Applied",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Third Party Account",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Mismatch of Bid amount with Amount Blocked",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Application by Retail in QIB category",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Bids below Cut off Price",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Bids by Partnership Firm",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Bid from Banks other than 52 UPI Notified Banks",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Applicants with US Address",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Rejected by Exchange",  Status = "Active"},
                new DatasetModel { Id = 5, bankName = "Employee bids above 500000",  Status = "Active"},
                
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
}
