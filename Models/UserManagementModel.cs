using System.Data;

namespace IPOWeb.Models
{
    public class UserManagementModel
    {
        public int? id { get; set; }
        public string empcode { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string userrole { get; set; }
        public string password { get; set; }
        public string pan { get; set; }
    }
}
