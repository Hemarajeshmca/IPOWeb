using DocumentFormat.OpenXml.Bibliography;
using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IPOWeb.Controllers
{
    public class UserGroupsMappingController : Controller
    {
        private IConfiguration _configuration;
        public UserGroupsMappingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string APIcookieName = "";
        string urlstring = "";
        public IActionResult UserGroupsMapping()
        {
            return View();
        }

        public JsonResult UserGroups()
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "UserGroups";
            DataSet result = new DataSet();
            string post_data = "";
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
                    var response = client.GetAsync(urlstring).Result;
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string _data1 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data1);
                    string _data = JsonConvert.SerializeObject(result.Tables[0]);
                    return Json(new { _data });
                }

            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateUserGroupsNew(
          string role_id,
          string role_name,
          string role_code,
          string app_code,
          string role_status)
        {
            string user_id = "0";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string apiUrl = _configuration
                        .GetSection("Appsettings")["apiurl"];

                    // Token
                    string cookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[cookieName];

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    // Send as POST BODY
                    var values = new Dictionary<string, string>
            {
                { "role_id", role_id ?? "0" },
                { "role_name", role_name ?? "" },
                { "role_code", role_code ?? "" },
                { "user_id", user_id },
                { "app_code", app_code ?? "" },
                { "role_status", role_status ?? "" }
            };

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(
                        apiUrl + "CreateUserGroupsNew",
                        content);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);

                        return Json(new
                        {
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }

                    var apiResult = await response.Content.ReadAsStringAsync();

                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(apiResult);

                    return Json(new
                    {
                        result = json.msg == 1,
                        msg = json.result.ToString()
                    });

                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    msg = ex.Message
                });
            }
        }

        [HttpPost]
        public ActionResult RoleMapping(string role_code, string role_name, string app_code, string app_name, string mode_)
        {
            ViewBag.role_code = role_code;
            ViewBag.role_name = role_name;
            ViewBag.app_code = app_code;
            ViewBag.app_name = app_name;
            ViewBag.mode_flag = mode_;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RoleMappingData_(string role_code, string app_code)
        {
            try
            {
                string urlstring = $"{_configuration["Appsettings:apiurl"]}RoleMapping?role_code={role_code}&app_code={app_code}";
                using (var client = new HttpClient())
                {
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
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    // Since API already returns JSON array
                    //  var data = JsonConvert.DeserializeObject<object>(json);

                    return Content(json, "application/json");
                }

            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [HttpGet]

        public async Task<IActionResult> Application_List()
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "Application_List";
            List<ApplicationModel> distinctRoles = new List<ApplicationModel>();

            try
            {
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

                    var response = await client.GetAsync(urlstring);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var post_datas = await response.Content.ReadAsStringAsync();
                        var parsed = JObject.Parse(post_datas);
                        var tables = parsed["table"].ToObject<List<ApplicationModel>>();

                        distinctRoles = tables
                            .GroupBy(r => r.app_code)
                            .Select(g => new ApplicationModel
                            {
                                app_code = g.First().app_code,
                                app_name = g.First().app_name
                            })
                            .ToList();
                    }
                }

                return Json(distinctRoles); // ✅ JSON, not View
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRolePermissions([FromBody] List<RolePermissionDto> permissions)
        {
            if (permissions == null || !permissions.Any())
                return Json(new { status = false, message = "No data received" });

            string apiUrl = _configuration.GetSection("Appsettings")["apiurl"] + "SaveRolePermissions";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    // Attach Bearer token from cookie
                    string apiCookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value + "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value;

                    string token = Request.Cookies[apiCookieName];
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(permissions),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync(apiUrl, jsonContent);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(apiCookieName);
                        return Json(new
                        {
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var parsed = JObject.Parse(responseData);
                        // Forward API response to frontend
                        return Json(parsed);
                    }
                    else
                    {
                        return Json(new { status = false, message = "API Error: " + response.StatusCode });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
    public class RolePermissionDto
    {
        //public int menu_id { get; set; }
        public string menu_code { get; set; }
        public string Add { get; set; }
        public string Modify { get; set; }
        public string Delete { get; set; }
        public string View { get; set; }
        public string Download { get; set; }
        public string Link { get; set; }
        public string Mail { get; set; }
        public string RetReq { get; set; }
        public string Approve { get; set; }
        public string Boachecklist { get; set; }
        public string role_code { get; set; }
        public string app_code { get; set; }
    }
}
