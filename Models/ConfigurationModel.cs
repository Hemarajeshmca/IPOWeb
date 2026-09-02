using System.Data;

namespace IPOWeb.Models
{
    public class ConfigurationModel
    {
        public string user_id { get; set; }
        public string password_max_len { get; set; }
        public string password_min_len { get; set; }
        public string password_attempt_count { get; set; }
        public string pwd_require_uppercase { get; set; }
        public string pwd_require_lowercase { get; set; }
        public string pwd_require_number { get; set; }
        public string pwd_require_special_char { get; set; }
        public string pwd_history_count { get; set; }
        public string pwd_lockout_duration_minutes { get; set; }
        public string screen_session_timeout { get; set; }

    }

}
