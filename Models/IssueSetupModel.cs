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
}
