using System.Data;

namespace IPOWeb.Models
{
    public class ClientSetupModel
    {
        public string in_user_code { get; set; }
        public string in_master_code { get; set; }

    }

    //public class Qcdgridread
    //{
    //    public string in_user_code { get; set; }
    //    public string in_master_code { get; set; }
    //}

    public class clientDetails
    {
        public int in_client_gid { get; set; }
        public string in_client_name { get; set; }
        public string in_client_type { get; set; }
        public string in_client_cin { get; set; }
        public string in_client_contact_person { get; set; }
        public string in_client_mob_no { get; set; }
        public string in_client_email_id { get; set; }
        public string in_client_addr { get; set; }
        public string in_client_country { get; set; }
        public string in_client_state { get; set; }
        public string in_client_city { get; set; }
        public string in_client_pincode { get; set; }
        public string in_insert_by { get; set; }
        public string in_update_by { get; set; }
        public string in_action { get; set; }
    }

    public class clientList
    {
        public int p_client_gid { get; set; }
        public string p_action { get; set; }
     
    }
  
}
