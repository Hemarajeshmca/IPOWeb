using DocumentFormat.OpenXml.Bibliography;
using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace IPOWeb.Controllers
{
    public class RoleMasterController : Controller
    {
        private IConfiguration _configuration;
        public RoleMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string APIcookieName = "";
        string urlstring = "";
        public IActionResult RoleMaster()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> RoleList()

        {
            List<RoleMasterModel> querylist = new List<RoleMasterModel>();
            string urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "RoleList";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(urlstring);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var post_data = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(post_data);

                        // Parse the JSON using JObject and convert to model
                        var parsed = JObject.Parse(post_data);
                        var table = parsed["table"].ToObject<List<RoleMasterModel>>();

                        querylist = table;
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex + " Server error"
                });
            }

            // return Json(querylist);
            return Json(new
            {
                success = true,
                data = querylist
            });
        }
        public JsonResult getallRolemaster([FromBody] RoleMasterModel context)
        {
            //string ss = HttpContext.Items["Userinfo"]?.ToString() ?? "";
            urlstring = _configuration.GetSection("Appsettings")["apiurl"].ToString();
            DataTable result = new DataTable();
            List<RoleMasterModel> objcat_lst = new List<RoleMasterModel>();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string Urlcon = "api/Rolemaster/";
                    client.BaseAddress = new Uri(urlstring + Urlcon);
                    //client.BaseAddress = new Uri("http://localhost:4195/api/Qcdmaster/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Add("lang_code", _configuration.GetSection("AppSettings")["lang_code"].ToString());
                    client.DefaultRequestHeaders.Add("ipaddress", _configuration.GetSection("AppSettings")["ipaddress"].ToString());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    // APIcookieName = userinfo.UserId + "-" + userinfo.UserRole;
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = client.PostAsync("getallRolelist", content).Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string d2 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataTable>(d2);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        RoleMasterModel objcat = new RoleMasterModel();
                        objcat.role_gid = Convert.ToInt32(result.Rows[i]["role_gid"]);
                        objcat.role_code = result.Rows[i]["role_code"].ToString();
                        objcat.role_name = result.Rows[i]["role_name"].ToString();
                        objcat.application_gid = Convert.ToInt32(result.Rows[i]["application_gid"]);
                        objcat_lst.Add(objcat);
                    }
                    return Json(objcat_lst);
                }
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "getallRolelist");
                return Json(ex.Message);
            }
        }


        public async Task<IActionResult> UserRoleList()
        {
            List<Dictionary<string, object>> querylist = new();

            string urlstring = _configuration["Appsettings:apiurl"] + "GetUserrole_Mapping";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    APIcookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[APIcookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(urlstring);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);

                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var post_data = await response.Content.ReadAsStringAsync();

                        querylist = JsonConvert
                            .DeserializeObject<List<Dictionary<string, object>>>(post_data);
                    }
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

            return Json(new
            {
                success = true,
                data = querylist
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserRoles_old([FromBody] SaveUserRolesRequest request)
        {
            List<Dictionary<string, object>> querylist = new();
            string urlstring = _configuration["Appsettings:apiurl"] + "SaveUserRoles";
            // using var client = new HttpClient();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                APIcookieName = "APItoken-" +
                    User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                    User.FindFirst(ClaimTypes.Role)?.Value;

                string token = Request.Cookies[APIcookieName];

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(urlstring, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Response.Cookies.Delete(APIcookieName);

                    return Json(new
                    {
                        success = false,
                        authExpired = true
                    });
                }

                if (response.IsSuccessStatusCode)
                {
                    var post_data = await response.Content.ReadAsStringAsync();

                    querylist = JsonConvert
                        .DeserializeObject<List<Dictionary<string, object>>>(post_data);
                }

                return Json(new { success = response.IsSuccessStatusCode });
            }
            return Json(new
            {
                success = true,
                data = querylist
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserRoles([FromBody] SaveUserRolesRequest request)
        {


            string urlstring = _configuration["Appsettings:apiurl"] + "SaveUserRoles";

            using (var client = new HttpClient())
            {



                client.DefaultRequestHeaders.Accept.Clear();
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                APIcookieName = "APItoken-" +
                    User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                    User.FindFirst(ClaimTypes.Role)?.Value;

                string token = Request.Cookies[APIcookieName];

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(urlstring, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Response.Cookies.Delete(APIcookieName);

                    return Json(new
                    {
                        success = false,
                        authExpired = true
                    });
                }
                return Json(new
                {
                    success = response.IsSuccessStatusCode
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetUserRole(string userCode)
        {
            List<UserRoleMappingModel> querylist = new List<UserRoleMappingModel>();
            string urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "GetUserRole";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(urlstring + "?userCode=" + userCode);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var post_data = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(post_data);
                        querylist = JsonConvert.DeserializeObject<List<UserRoleMappingModel>>(post_data);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex + " Server error"
                });
            }
            return Json(new
            {
                success = true,
                data = querylist
            });

        }
    }
}
