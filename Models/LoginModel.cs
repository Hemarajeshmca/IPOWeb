using System.Data;

namespace IPOWeb.Models
{
    public class LoginModel
    {
        public int empid { get; set; }
        public string app_code { get; set; }
        public string empCode { get; set; }
        public string txt_pwd { get; set; }
        public String? empEmail { get; set; }
		public String? Otp { get; set; }
        public string old_pwd { get; set; }
        public string new_pwd { get; set; }
        public string con_pwd { get; set; }

        public Int16 userGroupid { get; set; }
        public string? menuType { get; set; }
        public Int16 menuGid { get; set; }
		public string fileType { get; set; }
		public string reportName { get; set; }
		public partial class menu
        {
            public int menu_gid { get; set; }
            public string menu_name { get; set; }
            public string menu_url { get; set; }
            public int parent_menu_gid { get; set; }
            public int menu_order { get; set; }
            public bool rights_flag { get; set; }
            public string icon_path { get; set; }
        }
    }
}
