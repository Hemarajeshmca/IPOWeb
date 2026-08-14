using System.Data;

namespace IPOWeb.Models
{
    public class RejectionModel
    {
        public string ipo_code { get; set; }
        public string applno { get; set; }
        public string orderno { get; set; }
        public string panno { get; set; }
        public string qty { get; set; }
        public string shares { get; set; }
        public string amt { get; set; } 
        public string rule_code { get; set; } 
        public string addremarks { get; set; }
        public string rejremarks { get; set; }
        public string audit_flag { get; set; }
        public string flag { get; set; }

    }
   
}
