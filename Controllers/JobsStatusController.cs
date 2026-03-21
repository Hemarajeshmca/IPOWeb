using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IPOWeb.Controllers;
using System.Data;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;

namespace IPOWeb.Controllers
{
    public class JobsStatusController : Controller
    {
        private IConfiguration _configuration;
        string urlstring = "";
        string APIcookieName = "";
        public JobsStatusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult JobsStatus()
        {
            return View();
        }

        #region jobtypelist

        [HttpPost]
        public JsonResult jobtypelist()
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            DataTable result = new DataTable();
            List<jobtype> objcat_lst = new List<jobtype>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Utility/";
                    client.BaseAddress = new Uri(urlstring + Urlcon);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(""), UTF8Encoding.UTF8, "application/json");
                    var response = client.GetAsync("jobtype").Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        jobtype objcat = new jobtype();
                        objcat.jobtype_code = result.Rows[i]["jobtype_code"].ToString();
                        objcat.jobtype_desc = result.Rows[i]["jobtype_desc"].ToString();
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "jobtypelist");
                return Json(ex.Message);
            }
        }

        public class jobtype
        {
            public string? jobtype_desc { get; set; }
            public string? jobtype_code { get; set; }
        }
        #endregion

        #region getjobinprogresslist
        [HttpPost]
        public JsonResult getjobinprogresslist([FromBody] Jobstatusmodel context)
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            Jobstatusmodel objList = new Jobstatusmodel();
            DataTable result = new DataTable();
            List<Joblistmodel> objcat_lst = new List<Joblistmodel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Utility/";
                    client.BaseAddress = new Uri(urlstring);                   
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("user_code", context.in_user_code);
                    client.DefaultRequestHeaders.Add("lang_code", _configuration.GetSection("AppSettings")["lang_code"].ToString());
                    client.DefaultRequestHeaders.Add("role_code", _configuration.GetSection("AppSettings")["role_code"].ToString());
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration.GetSection("AppSettings")["ipaddress"].ToString());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync("jobinpogress", content).Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        Joblistmodel objcat = new Joblistmodel();
                        objcat.job_gid = Convert.ToInt32(result.Rows[i]["job_gid"]);
                        objcat.jobtype_code = result.Rows[i]["jobtype_code"].ToString();
                        objcat.job_name = result.Rows[i]["job_name"].ToString();
                        objcat.start_date = result.Rows[i]["start_date"].ToString();
                        objcat.end_date = result.Rows[i]["end_date"].ToString();
                        objcat.job_remark = result.Rows[i]["job_remark"].ToString();
                        objcat.jobstatus_desc = result.Rows[i]["jobstatus_desc"].ToString();
                        objcat.jobtype_desc = result.Rows[i]["jobtype_desc"].ToString();
                        objcat.job_initiated_by = result.Rows[i]["job_initiated_by"].ToString();
                        objcat.recon_code = result.Rows[i]["recon_code"].ToString();
                        objcat.recon_name = result.Rows[i]["recon_name"].ToString();
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "getjobinprogresslist");
                return Json(ex.Message);
            }
        }
        #endregion

        #region Joblistfetch
        [HttpPost]
        public JsonResult Joblistfetch([FromBody] Jobstatusmodel context)
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            Jobstatusmodel objList = new Jobstatusmodel();
            DataTable result = new DataTable();
            List<Joblistmodel> objcat_lst = new List<Joblistmodel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Utility/";
                    client.BaseAddress = new Uri(urlstring);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("user_code", context.in_user_code);
                    client.DefaultRequestHeaders.Add("lang_code", _configuration.GetSection("AppSettings")["lang_code"].ToString());
                    client.DefaultRequestHeaders.Add("role_code", _configuration.GetSection("AppSettings")["role_code"].ToString());
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration.GetSection("AppSettings")["ipaddress"].ToString());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync("jobcompleted", content).Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        Joblistmodel objcat = new Joblistmodel();
                        objcat.job_gid = Convert.ToInt32(result.Rows[i]["job_gid"]);
                        objcat.jobtype_code = result.Rows[i]["jobtype_code"].ToString();
                        objcat.job_name = result.Rows[i]["job_name"].ToString();
                        objcat.start_date = result.Rows[i]["start_date"].ToString();
                        objcat.end_date = result.Rows[i]["end_date"].ToString();
                        objcat.job_remark = result.Rows[i]["job_remark"].ToString();
                        objcat.jobstatus_desc = result.Rows[i]["jobstatus_desc"].ToString();
                        objcat.jobtype_desc = result.Rows[i]["jobtype_desc"].ToString();
                        objcat.recon_code = result.Rows[i]["recon_code"].ToString();
                        objcat.recon_name = result.Rows[i]["recon_name"].ToString();
                        objcat.file_type = result.Rows[i]["file_type"].ToString();
                        objcat.job_initiated_by = result.Rows[i]["job_initiated_by"].ToString();
                        objcat.file_name = result.Rows[i]["file_name"].ToString();
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "Joblistfetch");
                return Json(ex.Message);
            }
        }

        public class Joblistmodel
        {
            public int job_gid { get; set; }
            public String? jobtype_code { get; set; }
            public String? job_name { get; set; }
            public String? start_date { get; set; }
            public String? end_date { get; set; }
            public String? job_remark { get; set; }
            public String? jobstatus_desc { get; set; }
            public String? jobtype_desc { get; set; }
            public String? recon_code { get; set; }
            public String? recon_name { get; set; }
            public String? file_type { get; set; }
            public string? job_initiated_by { get; set; }
            public string? file_name { get; set; }

        }

        public class Jobstatusmodel
        {
            public String? in_start_date { get; set; }
            public String? in_end_date { get; set; }
            public String? in_jobtype_code { get; set; }
            public String? in_jobstatus { get; set; }
            public string? in_user_code { get; set; }
        }
        #endregion
        #region QueueListFetch
        [HttpPost]
        public JsonResult QueueListFetch([FromBody] Jobstatusmodel context)
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            Jobstatusmodel objList = new Jobstatusmodel();
            DataTable result = new DataTable();
            List<Queuelistmodel> objcat_lst = new List<Queuelistmodel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Utility/";
                    client.BaseAddress = new Uri(urlstring );
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("user_code", context.in_user_code);
                    client.DefaultRequestHeaders.Add("lang_code", _configuration.GetSection("AppSettings")["lang_code"].ToString());
                    client.DefaultRequestHeaders.Add("role_code", _configuration.GetSection("AppSettings")["role_code"].ToString());
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration.GetSection("AppSettings")["ipaddress"].ToString());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync("jobinQueue", content).Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        Queuelistmodel objcat = new Queuelistmodel();
                        objcat.in_koqueue_gid = Convert.ToInt32(result.Rows[i]["koqueue_gid"]);
                        objcat.recon_code = result.Rows[i]["recon_code"].ToString();
                        objcat.recon_name = result.Rows[i]["recon_name"].ToString();
                        objcat.scheduled_date = result.Rows[i]["scheduled_date"].ToString();
                        objcat.in_koqueue_remark = result.Rows[i]["koqueue_remark"].ToString();
                        objcat.koqueue_status = result.Rows[i]["koqueue_status"].ToString();
                        objcat.jobstatus_desc = result.Rows[i]["jobstatus_desc"].ToString();
                        objcat.scheduled_by = result.Rows[i]["scheduled_by"].ToString();
                        objcat.queue_type = result.Rows[i]["queue_type"].ToString();  // "Knock Off";
                        objcat.queue_name = result.Rows[i]["queue_name"].ToString();
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "Queuelistfetch");
                return Json(ex.Message);
            }
        }
        [HttpPost]
        public JsonResult QueueSuspend([FromBody] KoQueued context)
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            Jobstatusmodel objList = new Jobstatusmodel();
            DataTable result = new DataTable();
            List<Queuelistmodel> objcat_lst = new List<Queuelistmodel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Utility/";
                    client.BaseAddress = new Uri(urlstring + Urlcon);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Add("user_code", context.in_user_code);
                    client.DefaultRequestHeaders.Add("lang_code", _configuration.GetSection("AppSettings")["lang_code"].ToString());
                    client.DefaultRequestHeaders.Add("role_code", _configuration.GetSection("AppSettings")["role_code"].ToString());
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration.GetSection("AppSettings")["ipaddress"].ToString());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync("SuspendKoQueue", content).Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);

                    return Json(d2);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "QueueSuspend");
                return Json(ex.Message);
            }
        }
        public class Queuelistmodel
        {
            public int in_koqueue_gid { get; set; }
            public String? recon_code { get; set; }
            public String? recon_name { get; set; }
            public String? scheduled_date { get; set; }
            public String? in_koqueue_remark { get; set; }
            public String? koqueue_status { get; set; }
            public String? jobstatus_desc { get; set; }
            public string? scheduled_by { get; set; }
            public string? in_user_code { get; set; }
            public string? queue_type { get; set; }
            public string? queue_name { get; set; }

        }
        public class KoQueued
        {
            public string? in_koqueue_remark { get; set; }
            public int in_koqueue_gid { get; set; }
            public string? in_user_code { get; set; }

        }
        #endregion
        #region Downloads
        public List<fileconfigmodel> getfilepath(string confing_val, string username)
        {
            string urlstring = Convert.ToString(_configuration["Appsettings:apiurl"]) + "configvalue";

            fileconfigmodel FileDownload = new fileconfigmodel();
            var context = _configuration["Appsettings:" + confing_val];
            FileDownload.in_config_name = context;

            try
            {
                using (var client = new HttpClient())
                {
                    // client.DefaultRequestHeaders.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Add("user_code", username);
                    client.DefaultRequestHeaders.Add("lang_code", _configuration["AppSettings:lang_code"]);
                    client.DefaultRequestHeaders.Add("role_code", _configuration["AppSettings:role_code"]);
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration["AppSettings:ipaddress"]);

                    var json = JsonConvert.SerializeObject(FileDownload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = client.PostAsync(urlstring, content).Result;

                    var post_data = response.Content.ReadAsStringAsync().Result;

                    // ✅ Direct conversion
                    var result = JsonConvert.DeserializeObject<List<fileconfigmodel>>(post_data);

                    return result ?? new List<fileconfigmodel>();
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "getfilepath");
                return new List<fileconfigmodel>();
            }
        }

        public IActionResult Downloads(string jobid, string filetype, string file_name, string username)
        {
            var myObjects = getfilepath("fileconfig_value", username);
            string filepath = "";
            if (myObjects != null && myObjects.Count > 0)
            {
                filepath = myObjects[0].out_config_value;
            }

            string urlstring = _configuration["Appsettings:filedownload"];

            fileModel FileDownloadgrid = new fileModel
            {
                jobGid = jobid,
                jobName = "",
                filePath = filepath?.Replace("'", "")
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(urlstring);
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(
                        JsonConvert.SerializeObject(FileDownloadgrid),
                        Encoding.UTF8,
                        "application/json");

                    content.Headers.Add("user_code", username);

                    var response = client.PostAsync("files", content).Result;

                    // ================= NON-XLSX FILE =================
                    if (filetype != "xlsx")
                    {
                        var bytes = response.Content.ReadAsByteArrayAsync().Result;
                        string zipName = file_name + ".zip";

                        return File(bytes, "application/octet-stream", zipName);
                    }

                    // ================= XLSX FILE =================
                    else
                    {
                        var obj_outresult = getfilepath("download_xls_folder", username);

                        string out_filepath = "";
                        if (obj_outresult != null && obj_outresult.Count > 0)
                        {
                            out_filepath = obj_outresult[0].out_config_value;
                        }

                        string fileName = file_name.ToLower().Contains(".xlsx")
                            ? file_name
                            : file_name + ".xlsx";

                        string filePath = Path.Combine(out_filepath, fileName);
                        if (!System.IO.File.Exists(filePath))
                        {
                            filePath = filePath.Replace("xlsx", "xlsm");

                            if (!System.IO.File.Exists(filePath))
                                return NotFound();
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                            {
                                var entry = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Optimal);

                                using (var entryStream = entry.Open())
                                using (var fileStream = System.IO.File.OpenRead(filePath))
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return File(memoryStream.ToArray(), "application/zip", file_name + ".zip");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "files");
                return Json(ex.Message);
            }
        }
        public class fileModel
        {
            public String? jobGid { get; set; }
            public string? jobName { get; set; }
            public String? filePath { get; set; }
        }
        public class fileconfigmodel
        {
            public string? in_config_name { get; set; }
            public string? out_config_value { get; set; }
            public string? out_msg { get; set; }
            public string? out_result { get; set; }
        }

        #endregion

        public (string fileType, byte[] archiveData, string archiveName) DownloadFiles(string subDirectory, string y)
        {
            string jobid = y;
            int filelength = jobid.Length;
            var zipName = $"archive-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";
            var files = Directory.GetFiles(subDirectory).ToList();
            string supportedExtensions = String.Concat(jobid.ToString(), ".csv,", jobid.ToString(), "_*.*");

            List<string> myList = new List<string>();

            foreach (string file in Directory.GetFiles(subDirectory, String.Concat(jobid, ".*"), SearchOption.AllDirectories).Union(
                                    Directory.GetFiles(subDirectory, String.Concat(jobid, "_*.*"), SearchOption.AllDirectories)))
            {

                var fil = Path.GetFileName(file);
                string filess = file;
                myList.Add(filess);

            }


            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    myList.ForEach(file =>
                    {
                        var filename = Path.GetFileName(file);
                        var theFile = archive.CreateEntry(filename);
                        using (var streamWriter = new StreamWriter(theFile.Open()))
                        {
                            streamWriter.Write(System.IO.File.ReadAllText(file));
                        }
                    });
                }
                return ("application/zip", memoryStream.ToArray(), zipName);
            }

        }

   
    }
}
