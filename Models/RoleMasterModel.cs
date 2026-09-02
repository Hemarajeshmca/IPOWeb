namespace IPOWeb.Models
{
    public class RoleMasterModel
    {
        public int role_gid { get; set; }
        public string role_code { get; set; } 
        public string role_name { get; set; }
        public int application_gid { get; set; }
        public string application_name { get; set; }
        public string application_code { get; set; }
        public string active_status { get; set; }
    }

    public class SaveUserRolesRequest
    {
        public string user_code { get; set; }
        public List<UserRoleMappingModel> roles { get; set; }
    }
    public class UserRoleMappingModel
    {
        public string app_code { get; set; }
        public string role_code { get; set; }
    }

}
