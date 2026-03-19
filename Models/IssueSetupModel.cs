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
        public string? action { get; set; }
        public string? offer_header_gid { get; set; }
        public string? offer_code { get; set; }
        public string? offer_type { get; set; }
        public string? offer_listing_no { get; set; }
        public string? offer_isin { get; set; }
        public string? offer_status { get; set; }
        public string? offer_remarks { get; set; }
        public string? client_code { get; set; }
        public string? user_code { get; set; }
        public char active_status { get; set; }
        public char delete_flag { get; set; }

    }

    public class OfferDetailsModel
    {
        public int? offer_detail_gid { get; set; }
        public decimal? offer_precapital { get; set; } = 0;
        public int offer_issuesize { get; set; } = 0;
        public decimal offer_postcapital { get; set; } = 0;
        public int? offer_lotsize { get; set; }
        public decimal? offer_facevalue { get; set; } = 0;
        public decimal? offer_premiun { get; set; } = 0;
        public string? offer_pricetype { get; set; }
        public decimal? offer_fixedprice { get; set; } = 0;
        public decimal? offer_maximumprice { get; set; } = 0;
        public decimal? offer_minimumprice { get; set; } = 0;
        public decimal? offer_cutoffprice { get; set; } = 0;
        public string? offer_code { get; set; }
        public string? client_code { get; set; }
        public string? user_code { get; set; }
        public string? action { get; set; }
    }
}
