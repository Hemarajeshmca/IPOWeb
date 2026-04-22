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
        public List<CategoryINDData> categoryINDData { get; set; }
        public List<CategoryCo> categoryCo { get; set; }
        public List<CategoryNRA10L> categoryNRA10L { get; set; }
        public List<CategoryNRB10L> categoryNRB10L { get; set; }
        public List<BankUPIData> bankUPIData { get; set; }
        public bidApplRcdData bidApplRcd { get; set; }
        public List<ValidAppln> validAppln { get; set; }
        public List<AllotmentSummary> allotmentSummary { get; set; }
        public List<BankMaster> bankMaster { get; set; }
        public List<CategoryQIB> categoryQIB { get; set; }
        public List<CategoryMM> categoryMM { get; set; }
        public List<CategoryEMP> categoryEMP { get; set; }
        public List<CategoryNIIC> categoryNIIC { get; set; }
        public List<CategorySOA> categorySOA { get; set; }
        public List<CategoryEXMMSOA> categoryEXMMSOA { get; set; }

        public List<CategoryMARMAK> categoryMARMAK { get; set; }
    }

    public class SummaryData
    {
        public string offer_code { get; set; }
        public string client_code { get; set; }
        public string client_name { get; set; }
        public long offer_issuesize { get; set; }
        public int offer_facevalue { get; set; }
        public int offer_premiun { get; set; }
        public int offer_fixedprice { get; set; }
        public long total_iposize { get; set; }
        public long mm_shares { get; set; }
        public long total_mm { get; set; }
        public long public_shares { get; set; }
        public long net_issue { get; set; }
        public string offer_openingdate { get; set; }
        public string offer_closingdate { get; set; }
    }

    public class BankData
    {
        public string bnk_code { get; set; }
        public string? bank_name { get; set; }
        public long bnk_appl_count { get; set; }
        public long bnk_quantity { get; set; }
        public decimal bank_amount { get; set; }
    }

    public class RejectionData
    {
        public string? rejected_reason { get; set; }
        public long total_quantity { get; set; }
        public long rejection_count { get; set; }
    }

    public class CategoryData
    {
        public string ipo_category { get; set; }
        public int total_appl { get; set; }
        public decimal quantity { get; set; }
        public decimal total { get; set; }
    }

    public class CategoryINDData
    {
        public int offer_cat_shares { get; set; }
        public int total_appl { get; set; }
        public int total_quantity { get; set; }
        public decimal? times_subs { get; set; }
    }
    public class CategoryCo
    {
        public int offer_cat_shares { get; set; }
        public int total_appl { get; set; }
        public int total_quantity { get; set; }
        public decimal? times_subs { get; set; }
    }

    public class CategoryNRA10L
    {
        public int offer_cat_shares { get; set; }
        public int total_appl { get; set; }
        public int total_quantity { get; set; }
        public decimal? times_subs { get; set; }
    }

    public class CategoryNRB10L
    {
        public int offer_cat_shares { get; set; }
        public int total_appl { get; set; }
        public int total_quantity { get; set; }
        public decimal? times_subs { get; set; }
    }

    public class BankUPIData
    {
        public string? bank_name { get; set; }
        public int no_of_bids { get; set; }
        public int no_of_shares_applied { get; set; }
        public long total_amount { get; set; }
    }

    public class bidApplRcdData
    {
        public long asba_total_bids { get; set; }
        public long asba_total_quantity { get; set; }
        public long nonasba_total_bids { get; set; }
        public long nonasba_total_quantity { get; set; }
        public long upi_total_bids { get; set; }
        public long upi_total_quantity { get; set; }
        public long total_bids { get; set; }
        public long total_quantity { get; set; }
        public long diff_bids { get; set; }
        public long diff_quantity { get; set; }
        public long diff1 { get; set; }
        public long diff2 { get; set; }
        public long banknotbidbids { get; set; }
        public long banknotbidqty { get; set; }
        public long bank_bids { get; set; }
        public long bank_bids_qty { get; set; }
    }

    public class ValidAppln
    {
        public string? ipo_category { get; set; }
        public long gross_appln { get; set; }
        public long gross_shares { get; set; }
        public long valid_appln { get; set; }
        public long valid_shares { get; set; }
        public long rejected_appln { get; set; }
        public long rejected_shares { get; set; }
    }

    public class AllotmentSummary
    {
        public string? ipo_category { get; set; }
        public long gross_appln { get; set; }
        public long gross_shares { get; set; }
        public long valid_appln { get; set; }
        public long valid_shares { get; set; }
        public long rejected_appln { get; set; }
        public long rejected_shares { get; set; }
        public long allotment_appln { get; set; }
        public long allotment_shares { get; set; }
    }

    public class BankMaster
    {
        public string? bank_name { get; set; }
    }

    public class CategoryQIB
    {
        public string bank_name { get; set; }
        public int no_of_applications { get; set; }
        public long no_of_shares { get; set; }
        public long total_amount { get; set; }
    }

    public class CategoryMM
    {
        public string bank_name { get; set; }
        public int no_of_applications { get; set; }
        public long no_of_shares { get; set; }
        public long total_amount { get; set; }
    }

    public class CategoryEMP
    {
        public string bank_name { get; set; }
        public int no_of_applications { get; set; }
        public long no_of_shares { get; set; }
        public long total_amount { get; set; }
    }

    public class CategoryNIIC
    {
        public string? Particulars { get; set; }
        public long nii_no_of_applications { get; set; }
        public long nii_no_of_shares { get; set; }
        public long ind_no_of_applications { get; set; }
        public long ind_no_of_shares { get; set; }
        public long total_no_of_applications { get; set; }
        public long total_no_of_shares { get; set; }      
    }

    public class CategorySOA
    {
        public string? ipo_category { get; set; }
        public long offer_cat_shares { get; set; }
        public long valid_shares_received { get; set; }
        public long equity_shares_allotted { get; set; }
        public long total_allotment_amount { get; set; }        
    }

    public class CategoryEXMMSOA
    {
        public string? ipo_category { get; set; }
        public long offer_cat_shares { get; set; }
        public long valid_shares_received { get; set; }
        public long equity_shares_allotted { get; set; }
        public long total_allotment_amount { get; set; }
    }

    public class CategoryMARMAK
    {
        public int mm_offer_cat_shares { get; set; }
        public int mm_total_appl { get; set; }
        public int mm_total_quantity { get; set; }
        public decimal? mm_times_subs { get; set; }
    }

}
