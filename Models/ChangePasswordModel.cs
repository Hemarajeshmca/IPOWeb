using System.Data;

namespace IPOWeb.Models
{
    public class ChangePasswordModel
    {
        public int? id { get; set; }
        public string empcode { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string userrole { get; set; }
        public string password { get; set; }
        public string pan { get; set; }
        public string usermobile { get; set; }
        public string userpwd { get; set; }
        public string userotp { get; set; }
        public string role_code { get; set; }
        public string role_name { get; set; }
        public string isclient { get; set; }
        public string userpwdexpdays { get; set; }
        public string lock_flag { get; set; }

    }
}
