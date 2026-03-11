using System.Data;

namespace IPOWeb.Models
{
    public class IssueSetupModel
    {
        public int mastergid { get; set; }
        public string mastercode { get; set; }
        public string mastername { get; set; }

        public string dependvalue { get; set; }

    }

    public class Qcdgridread
    {
        public string in_user_code { get; set; }
        public string in_master_code { get; set; }
    }

    public class OfferHeaderModel
    {
        public string? in_action { get; set; }
        public string? in_offer_header_gid { get; set; }
        public string? in_offer_type { get; set; }
        public string? in_offer_listing { get; set; }
        public string? in_offer_isin { get; set; }
        public string? in_offer_status { get; set; }
        public string? in_offer_remarks { get; set; }
        public string? in_client_code { get; set; }
        public string? in_active_status { get; set; }
        public string? in_user_code { get; set; }


    }
}
