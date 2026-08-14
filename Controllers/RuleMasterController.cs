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
    public class RuleMasterController : Controller
    {
        private IConfiguration _configuration;
        public RuleMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        string APIcookieName = "";
        public IActionResult Rulemaster()
        {
            return View();
        }

        #region allRulemaster
        [HttpPost]

        public async Task<JsonResult> GetAllRuleMaster(string ipo_code) 
        {
            string urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "getallRulemaster?ipo_code=" + ipo_code;
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
        public async Task<JsonResult> SaveAppliedRules([FromBody] SaveRuleRequest request)
        {
            try
            {
                string urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "SaveAppliedRules";

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
                        rule_code = request.rule_code,
                        remarks = request.remarks
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
                        var result = await response.Content.ReadAsStringAsync();

                        return Json(new
                        {
                            success = true,
                            data = result
                        });
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
        public class rulegridread
        {
            public string in_user_code { get; set; }
            public string in_rule_code { get; set; }
            public string in_rule_name { get; set; }
        }
        public class rulemodel
        { 
            public string in_rule_code { get; set; }
            public string in_rule_name { get; set; }
        }
        public class SaveRuleRequest
        {
            public string ipo_code { get; set; }
            public string rule_code { get; set; }
            public string remarks { get; set; }
            //public string insert_by { get; set; }
            
        }
    }
}
