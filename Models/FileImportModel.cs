using System.Data;

namespace IPOWeb.Models
{
    public class FileImportModel
    {
        public int dataset_gid { get; set; }
        public string dataset_code { get; set; }
        public string dataset_category { get; set; }
        public string dataset_table_name { get; set; }
        public string system_flag { get; set; }

    }
}
