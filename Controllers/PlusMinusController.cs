using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using static IPOWeb.Controllers.RuleMasterController;

namespace IPOWeb.Controllers
{
    public class PlusMinusController : Controller
    {
        private IConfiguration _configuration;
        public PlusMinusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult PlusMinus()
        {
            return View();
        }
        #region FetchPlusMinus
        [HttpPost] 
        public async Task<JsonResult> FetchPlusMinus(string ipo_code)
        {
            string urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "FetchPlusMinus?ipo_code=" + ipo_code;
            DataTable result = new DataTable();
            List<rulemodel> objcat_lst = new List<rulemodel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    string APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value + "_" + User.FindFirst(ClaimTypes.Role)?.Value;
                    string token = Request.Cookies[APIcookieName];
                    if (string.IsNullOrEmpty(token))
                    {
                        return Json(new { success = false, authExpired = true });
                    }

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent("", Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(urlstring, content);
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);
                    // 🔐 Unauthorized
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new { success = false, authExpired = true });
                    }

                    // ✅ Success
                    if (response.IsSuccessStatusCode)
                    {
                        var resultMessage = await response.Content.ReadAsStringAsync();
                        var companyData = JsonConvert.DeserializeObject<object>(resultMessage);
                        return Json(new { success = true, data = companyData });
                    }

                    // ❌ Failure
                    return Json(new
                    {
                        success = false,
                        message = "API call failed: " + response.StatusCode
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
        [HttpPost]
        public async Task<JsonResult> ApprovePlusMinus([FromBody] plusminusModel request)
        {
            try
            {
                string urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "saveAddRejDetails";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    string APIcookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[APIcookieName];

                    if (string.IsNullOrEmpty(token))
                    {
                        return Json(new { success = false, authExpired = true });
                    }

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var postData = new
                    {
                        ipo_code = request.ipo_code,
                        applno = request.applno,
                        rejremarks = request.rejremarks,
                        audit_flag = request.audit_flag,
                        flag = request.flag,
                    };

                    var content = new StringContent(
                        JsonConvert.SerializeObject(postData),
                        Encoding.UTF8,
                        "application/json");

                    var response = await client.PostAsync(urlstring, content);

                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, APIcookieName);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new { success = false, authExpired = true });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        //var result = await response.Content.ReadAsStringAsync();

                        //return Json(new
                        //{
                        //    success = true,
                        //    data = result
                        //});
                        string post_data = "";
                        DataSet result = new DataSet();
                        Stream data = response.Content.ReadAsStreamAsync().Result;
                        StreamReader reader = new StreamReader(data);
                        post_data = reader.ReadToEnd();
                        string _data1 = JsonConvert.DeserializeObject<string>(post_data);
                        result = JsonConvert.DeserializeObject<DataSet>(_data1);
                        string _data = JsonConvert.SerializeObject(result.Tables[0]);
                        return Json(new { _data });
                    }

                    return Json(new
                    {
                        success = false,
                        message = response.ReasonPhrase
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public class plusminusModel
        {
            public string ipo_code { get; set; }
            public string applno { get; set; }
            public string orderno { get; set; }
            public string panno { get; set; }
            public string qty { get; set; }
            public string shares { get; set; }
            public string amt { get; set; }
            public string rule_code { get; set; }
            public string addremarks { get; set; }
            public string rejremarks { get; set; }
            public string audit_flag { get; set; }
            public string flag { get; set; }

        }
    }
}
