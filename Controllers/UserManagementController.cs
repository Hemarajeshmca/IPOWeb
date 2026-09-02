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
    public class UserManagementController : Controller
    {
        private IConfiguration _configuration;
        public UserManagementController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string APIcookieName = "";
        string urlstring = "";
        public IActionResult UserManagement()
        {
            return View();
        }

        public async Task<IActionResult> UsersList()
        {
            List<UserManagementModel> querylist = new List<UserManagementModel>();
            string urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "UsersList";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync(urlstring);
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

                    if (response.IsSuccessStatusCode)
                    {
                        var post_data = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(post_data);

                        // Parse the JSON using JObject and convert to model
                        var parsed = JObject.Parse(post_data);
                        var table = parsed["table"].ToObject<List<UserManagementModel>>();

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

        [HttpGet]

        public async Task<IActionResult> RoleList()
        {
            urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "RoleList";
            List<RoleMasterModel> distinctRoles = new List<RoleMasterModel>();

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
                        var tables = parsed["table"].ToObject<List<RoleMasterModel>>();

                        distinctRoles = tables
                            .GroupBy(r => r.role_gid)
                            .Select(g => new RoleMasterModel
                            {
                                role_gid = g.First().role_gid,
                                role_code = g.First().role_code,
                                role_name = g.First().role_name
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


        public async Task<JsonResult> InsUser([FromBody] UserManagementModel mymodel)
        {
            var urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "InsUser";
            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(mymodel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(urlstring, content);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);
                        return Json(new
                        {
                            success = false,
                            authExpired = true
                            //message = "Session expired. Please login again."
                        });
                    }
                    Stream data = response.Content.ReadAsStreamAsync().Result;
                    StreamReader reader = new StreamReader(data);
                    post_data = reader.ReadToEnd();
                    string _data1 = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data1);
                    string _data = JsonConvert.SerializeObject(result.Tables[0]);

                    if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0 && result.Tables[0].Rows[0][0].ToString() == "Success" && Convert.ToInt32(result.Tables[0].Rows[0][2]) > 0)
                    {
                        // await AckEmail(mymodel);
                        //return Json(new { _data });

                    }
                    return Json(new { _data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task AckEmail([FromBody] UserManagementModel model)
        {
            try
            {
                // SMTP configuration
                var appSettings = _configuration.GetSection("Appsettings");

                string smtpServer = appSettings["smtpServer"];
                int smtpPort = int.Parse(appSettings["smtpPort"]);
                string smtpUsername = appSettings["smtpUsername"];
                string smtpPassword = appSettings["smtpPassword"];

                MailMessage mail = new MailMessage();
                var useremail = model.email;
                mail.To.Add(useremail);
                mail.From = new MailAddress(smtpUsername);
                mail.Subject = "STA Web - Login Credentials";
                var name = model.name;
                var empcode = model.empcode;
                mail.Body = $"Dear {name},<br/><br/>Your account has been successfully created. Please find your login credentials : <br/><br/> Employee Code: {empcode}  <br/><br/> Password: Gnsa@123 <br/><br/>Please login to our STA Web portal to change your password.<br/><br/>Link : <a href='http://localhost:5025/'>http://localhost:5025/</a> <br/><br/>Thanks, <br><br>GNSA Infotech <br><br>This is an autogenerated mail, please do not reply to this mail.";
                mail.IsBodyHtml = true;

                // Create a SmtpClient object and send the email
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true; // Enable SSL/TLS encryption
                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                //return StatusCode(500, "Error: " + ex.Message);
            }
        }

        public async Task<IActionResult> UpdUser([FromBody] UserManagementModel mymodel)
        {
            var urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "UpdUser";
            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    //HttpContext.Items["Userinfo"] = APIcookieName;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(mymodel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

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
                    if (!response.IsSuccessStatusCode)
                        return StatusCode((int)response.StatusCode, "Login failed");

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
                return Json(new { success = false, message = ex.Message });
            }
        }




        public async Task<JsonResult> GetApplicationRoles()
        {
            string urlstring = _configuration.GetSection("Appsettings")["apiurl"] + "RoleList";

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

                    /* 🔐 AUTH EXPIRED */
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Response.Cookies.Delete(APIcookieName);

                        return Json(new
                        {
                            authExpired = true,
                            message = "Session expired. Please login again."
                        });
                    }

                    /* ✅ SUCCESS */
                    if (response.IsSuccessStatusCode)
                    {
                        var postData = await response.Content.ReadAsStringAsync();

                        var parsed = JObject.Parse(postData);

                        // 🔹 API returns: { table : [ ... ] }
                        var roles = parsed["table"].ToObject<List<RoleMasterModel>>();

                        // 🔹 Send BASE DATA ONLY
                        var result = roles.Select(r => new
                        {
                            role_code = r.role_code,
                            role_name = r.role_name,
                            application_code = r.application_code,
                            application_name = r.application_name
                        }).ToList();

                        return Json(new
                        {
                            authExpired = false,
                            data = result
                        });
                    }
                    return Json(new
                    {
                        authExpired = false,
                        data = new List<object>()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    authExpired = false,
                    error = ex.Message
                });
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
        public async Task<IActionResult> SaveUserRoles([FromBody] SaveUserRolesRequest request)
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

        [HttpGet]
        public async Task<IActionResult> GetPwdConfigValues()
        {
            PasswordConfigModel querylist = new PasswordConfigModel();

            string urlstring = _configuration
                .GetSection("Appsettings")["apiurl"] + "GetPwdConfigValues";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );

                    APIcookieName = "APItoken-" +
                        User.FindFirst(ClaimTypes.Name)?.Value.ToString() +
                        "_" +
                        User.FindFirst(ClaimTypes.Role)?.Value.ToString();

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

                        Console.WriteLine(post_data);

                        querylist = JsonConvert.DeserializeObject<PasswordConfigModel>(post_data);
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
