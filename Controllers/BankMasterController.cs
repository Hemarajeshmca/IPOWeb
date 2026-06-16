using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IPOWeb.Models;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace IPOWeb.Controllers
{
    public class BankMasterController : Controller
    {
        private IConfiguration _configuration;
        public BankMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";
        public IActionResult BankMaster()
        {
            return View();
        }

        #region allBankmaster
        [HttpPost]
        public JsonResult getallBankmaster([FromBody] Qcdgridread context)
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            DataTable result = new DataTable();
            List<BankMasterModel> objcat_lst = new List<BankMasterModel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    string Urlcon = "Bankmaster/";
                    client.BaseAddress = new Uri(urlstring);
                    //client.BaseAddress = new Uri("http://localhost:4195/api/Qcdmaster/");
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
                    var response = client.PostAsync("getallbankmaster",content).Result;
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        BankMasterModel objcat = new BankMasterModel();
                        objcat.bank_id = Convert.ToInt32(result.Rows[i]["bank_id"]);
                        objcat.bank_code = result.Rows[i]["bank_code"].ToString();
                        objcat.bank_name = result.Rows[i]["bank_name"].ToString();
                        objcat.ifsc_code = result.Rows[i]["ifsc_code"].ToString();
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "getallbankmaster");
                return Json(ex.Message);
            }
        }


        public class Qcdgridread
        {
            public string in_user_code { get; set; }
            public string in_master_code { get; set; }
        }
        #endregion
    }
}
