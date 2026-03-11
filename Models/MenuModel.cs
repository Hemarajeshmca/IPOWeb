using System.Data;

namespace IPOWeb.Models
{
    public class MenuModel
    {
        public string menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_url { get; set; }
        public string add_perm { get; set; }
        public string mod_perm { get; set; }
        public string view_perm { get; set; }
        public string delete_perm { get; set; }
        public string download_perm { get; set; }
        public string link_perm { get; set; }
        public string mail_perm { get; set; }
        public string retreq_perm { get; set; }
       public string deny_perm { get; set; }
       public string menu_type { get; set; }
    }
}
