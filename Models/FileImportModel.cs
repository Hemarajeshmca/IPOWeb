using System.Data;

namespace IPOWeb.Models
{
    public class FileImportModel
    {
        public int dataset_gid { get; set; }
        public string dataset_code { get; set; }
        public string dataset_name { get; set; }
        public string dataset_category { get; set; }
        public string dataset_table_name { get; set; }
        public string system_flag { get; set; }

    }

    public class PipelineModel
    {
        public int pipeline_gid { get; set; }
        public string pipeline_code { get; set; }
        public string pipeline_name { get; set; }
        public string pipeline_desc { get; set; }
        public string pipeline_status { get; set; }
        public string file_extenstion { get; set; }
        public string source_db_type { get; set; }
    }

    public class ImportStartModel
    {
       
        public string pipeline_code { get; set; }
        public string dataset_code { get; set; }
        public string initiated_by { get; set; }
        public string reference_no { get; set; }
        
    }
}
