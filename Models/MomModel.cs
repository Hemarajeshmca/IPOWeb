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
        public List<BankNAMaster> bankNAMaster { get; set; }
        public List<CategoryQIB> categoryQIB { get; set; }
        public List<CategoryMM> categoryMM { get; set; }
        public List<CategoryEMP> categoryEMP { get; set; }
        public List<CategoryNIIC> categoryNIIC { get; set; }
        public CategoryCNIIC categoryCNIIC { get; set; }
        public CategoryCQIB categoryCQIB { get; set; }
        public CategoryCMMS categoryCMMS { get; set; }
        public CategoryCRNR categoryCRNR { get; set; }
        public CategoryCOVERSUBS categoryCOVERSUBS { get; set; }
        public CategoryCANCH categoryCANCH { get; set; }
        public List<CategoryCSTK> categoryCSTK { get; set; }
        public List<CategorySOA> categorySOA { get; set; }
        public List<CategoryEXMMSOA> categoryEXMMSOA { get; set; }
        public List<CategoryMARMAK> categoryMARMAK { get; set; }
        public List<CategoryTechRej> categoryTechRej { get; set; }

        public categoryUPISummary categoryUPISummary { get; set; }
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
        public decimal issue_percentage { get; set; }
        public decimal net_issue_percentage { get; set; }
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
        public int? offer_cat_shares { get; set; }
        public int? total_appl { get; set; }
        public int? total_quantity { get; set; }
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

        public int total_bank_count { get; set; }

    }

    public class BankNAMaster
    {
        public string? nonasba_bank_name { get; set; }

        public int nonasba_total_bank_count { get; set; }

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

    public class CategoryCNIIC
    {
        public long ctotal_applications { get; set; }
        public string ctotal_shares { get; set; }
        public string ctotal_application_amount { get; set; }
    }

    public class CategoryCOVERSUBS
    {
        public long subs_net_appl_shares { get; set; }
        public decimal over_subs { get; set; }
        public decimal rej_over_subs { get; set; }
        public long subs_rejected_appl_shares { get; set; }
        public long subs_total_appl_shares { get; set; }
        public long subs_total_offer_shares { get; set; }
    }


    public class CategoryCANCH
    {
        public long mma_applications { get; set; }
        public long mma_applied_shares { get; set; }
        public long mma_amount { get; set; }
        public long mma_reserved_shares { get; set; }
        public decimal mm_offer_facevalue { get; set; }
        public decimal mm_offer_premiun { get; set; }
        public decimal mm_offer_fixedprice { get; set; }
        public long mma_allocated_amount { get; set; }
        public long mma_shares_available_for_public { get; set; }
        public string mm_offer_openingdate { get; set; }
        public string mm_offer_closingdate { get; set; }
        public string mm_offer_allotmentdate { get; set; }
        public string mm_offer_listingdate { get; set; }
        public decimal mma_bid_book_subscription { get; set; }
        public decimal mm_final_subscription { get; set; }
    }

    public class CategoryCSTK
    {
        public string stack_name { get; set; }
        public string stack_contact { get; set; }
        public string stack_designation { get; set; }
        public string stack_type { get; set; }
    }

    public class CategoryCRNR
    {
        public long rnr_valid_applications { get; set; }
        public long rnr_valid_shares { get; set; }       
        public long pan_mismatch_applications { get; set; }       
        public long pan_mismatch_shares { get; set; }       
        public long invalid_dp_applications { get; set; }       
        public long invalid_dp_shares { get; set; }       
        public long multi_pan_applications { get; set; }       
        public long multi_pan_shares { get; set; }       
        public long lsp_applications { get; set; }       
        public long lsp_shares { get; set; }       
    }

    public class CategoryCQIB
    {
        public long qibs_applications { get; set; }
        public long qibs_shares { get; set; }
        public long qibs_application_money { get; set; }
        public long qibs_reserved_shares { get; set; }
    }

    public class CategoryCMMS
    {
        public long mms_total_applications { get; set; }
        public long mms_amount_blocked { get; set; }
        public long mms_mm_applications { get; set; }
        public long mms_mm_shares { get; set; }
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

    public class CategoryTechRej
    {
        public long total_appls { get; set; }
        public long total_appl_shares { get; set; }
        public long total_appl_amount { get; set; }
        public long rejected_appls { get; set; }
        public long rejected_appl_shares { get; set; }
        public long rejected_appl_amount { get; set; }
        public long net_appls { get; set; }
        public long net_appl_shares { get; set; }
        public long net_appl_amount { get; set; }
    }

    public class categoryUPISummary
    {
        public long upisum_total_bids { get; set; }
        public long upisum_total_shares { get; set; }
        public long appl_blocked_bids { get; set; }
        public long appl_blocked_amount { get; set; }
        public long bid_reg_not_bank_bids { get; set; }
        public long bid_reg_not_bank_amount { get; set; }
        public long unique_appln { get; set; }
    }

    public class insertJobModel
    {
        public string? recon_code { get; set; }
        public string? jobtype_code { get; set; }
        public int job_ref_gid { get; set; }
        public string? job_name { get; set; }
        public string? job_input_param { get; set; }
        public string? job_initiated_by { get; set; }
        public string? ip_addr { get; set; }
        public string? job_status { get; set; }
        public string? job_remark { get; set; }

    }

    public class updateJobModel
    {
        public string? in_job_gid { get; set; }
        public string? in_job_status { get; set; }
        public string? in_job_remark { get; set; }
    }

    public class ReportRequest
    {
        public string offer_code { get; set; }
    }

    public class MomReportData
    {
        public List<Dictionary<string, object>> table1 { get; set; }
        public List<Dictionary<string, object>> table2 { get; set; }
        public List<Dictionary<string, object>> table3 { get; set; }
        public List<Dictionary<string, object>> table4 { get; set; }
        public List<Dictionary<string, object>> table5 { get; set; }
        public List<Dictionary<string, object>> table6 { get; set; }
        public List<Dictionary<string, object>> table7 { get; set; }
        public List<Dictionary<string, object>> table8 { get; set; }
        public List<Dictionary<string, object>> table9 { get; set; }
        public List<Dictionary<string, object>> table10 { get; set; }
        public List<Dictionary<string, object>> table11 { get; set; }
        public List<Dictionary<string, object>> table12 { get; set; }
        public List<Dictionary<string, object>> table13 { get; set; }
        public List<Dictionary<string, object>> table14 { get; set; }
        public List<Dictionary<string, object>> table15 { get; set; }
        public List<Dictionary<string, object>> table16 { get; set; }
        public List<Dictionary<string, object>> table17 { get; set; }
        public List<Dictionary<string, object>> table18 { get; set; }
        public List<Dictionary<string, object>> table19 { get; set; }
        public List<Dictionary<string, object>> table20 { get; set; }
        public List<Dictionary<string, object>> table21 { get; set; }
        public List<Dictionary<string, object>> table22 { get; set; }
        public List<Dictionary<string, object>> table23 { get; set; }
        public List<Dictionary<string, object>> table24 { get; set; }
        public List<Dictionary<string, object>> table25 { get; set; }
        public List<Dictionary<string, object>> table26 { get; set; }
        public List<Dictionary<string, object>> table27 { get; set; }
        public List<Dictionary<string, object>> table28 { get; set; }
        public List<Dictionary<string, object>> table29 { get; set; }
        public List<Dictionary<string, object>> table30 { get; set; }
        public List<Dictionary<string, object>> table31 { get; set; }
    }
}
