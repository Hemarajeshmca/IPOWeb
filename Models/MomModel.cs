using System.Data;

namespace IPOWeb.Models
{
    public class MomRequest
    {
        public SummaryData summary { get; set; }
        public List<BankData> bankData { get; set; }
        public List<BankData> nonasbabankData { get; set; }
        public List<RejectionData> rejectionData { get; set; }
        public List<CategoryData> categoryData { get; set; }
    }

    public class SummaryData
    {
        public string offer_code { get; set; }
        public string client_code { get; set; }
        public string client_name { get; set; }
        public int offer_issuesize { get; set; }
        public int offer_facevalue { get; set; }
        public int offer_premiun { get; set; }
        public int offer_fixedprice { get; set; }
        public int total_iposize { get; set; }
        public int mm_shares { get; set; }
        public int total_mm { get; set; }
        public int public_shares { get; set; }
        public int net_issue { get; set; }
    }

    public class BankData
    {
        public string bnk_code { get; set; }
        public string bank_name { get; set; }
        public int bnk_appl_count { get; set; }
        public int bnk_quantity { get; set; }
        public decimal bank_amount { get; set; }
    }

    public class RejectionData
    {
        public string rejected_reason { get; set; }       
        public int total_quantity { get; set; }
        public int rejection_count { get; set; }        
    }

    public class CategoryData
    {
        public string ipo_category { get; set; }
        public int total_appl { get; set; }
        public decimal quantity { get; set; }
        public decimal total { get; set; }
    }

}
