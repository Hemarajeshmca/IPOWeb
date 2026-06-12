using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IPOWeb.Controllers;
using IPOWeb.Models;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace IPOWeb.Controllers
{
    public class LoginController : Controller
    {
        private static IConfiguration _configuration;
        string _data = "";
        string APIcookieName = "";
        string set_Apitoken = "";

        //public LoginController()
        //{

        //}
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        string urlstring = "";
        public IActionResult Login()
        {
            HttpContext.Session.SetString("user_id", "1");
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult SessionExpired()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Users_login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.empCode) || string.IsNullOrEmpty(model.con_pwd))
                return Json(new { success = false, message = "Username & Password required" });

            try
            {
                string preTokenUrl = _configuration["Appsettings:apiurl"] + "/auth/generate-token";
                string loginApiUrl = _configuration["Appsettings:apiurl"] + "/User_loginvalidate";

                //using var client = new HttpClient();
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5) // or seconds
                };

                // 🔹 Step 1: Get pre-token from API
                var preTokenResponse = await client.PostAsync(preTokenUrl, null);
                if (!preTokenResponse.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Unable to get pre-token" });

                var preTokenJson = await preTokenResponse.Content.ReadAsStringAsync();
                var preToken = JObject.Parse(preTokenJson)["token"]?.ToString();
                if (string.IsNullOrEmpty(preToken))
                    return Json(new { success = false, message = "Pre-token not returned" });

                // 🔹 Step 2: Call login API with username, password, pre-token
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", preToken);

                var loginResponse = await client.PostAsync(loginApiUrl, content);
                if (!loginResponse.IsSuccessStatusCode)
                    return StatusCode((int)loginResponse.StatusCode, "Login failed");

                var rawJson = await loginResponse.Content.ReadAsStringAsync();
                var apiResult = JsonConvert.DeserializeObject<dynamic>(rawJson);

                var user = apiResult.user;
                string id = apiResult.user.id;
                string name = apiResult.user.name;
                string email = apiResult.user.email;
                string role = apiResult.user.role;
                string user_code = apiResult.user.user_code;
                string idrole = id + "_" + role;
                //userinfo.UserId = id.ToString();
                //userinfo.UserRole = role.ToString();
                set_Apitoken = idrole;
                ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, loginResponse, set_Apitoken);


                HttpContext.Session.SetString("user_id", id.ToString());
                HttpContext.Session.SetString("user_name", name.ToString());
                HttpContext.Session.SetString("user_role", role.ToString());
                HttpContext.Session.SetString("user_email", email.ToString());
                HttpContext.Session.SetString("user_code", user_code.ToString());

                // 🔹 Step 5: Cookie authentication for website
                var claims = new List<Claim>
         {
             new Claim(ClaimTypes.NameIdentifier, id.ToString()),
             new Claim(ClaimTypes.Name, user.role.ToString()),
             new Claim(ClaimTypes.Role, user.role.ToString()),
         };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)
                );

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard"), role = user.role.ToString(), message = "Login success" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> ChkLogin(string empCode, string txt_pwd, string app_code)
        {
            app_code = _configuration.GetSection("AppSettings")["App_code"];
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["cumsapiurl"]) + "/ChkLogin";


            string preTokenUrl = _configuration["Appsettings:cumsapiurl"] + "/auth/generate-token";
            string loginApiUrl = _configuration["Appsettings:cumsapiurl"] + "/ChkLogin";

            using var clients = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5) // or seconds
            };

            // 🔹 Step 1: Get pre-token from API
            var preTokenResponse = await clients.PostAsync(preTokenUrl, null);
            if (!preTokenResponse.IsSuccessStatusCode)
                return Json(new { success = false, message = "Unable to get pre-token" });

            var preTokenJson = await preTokenResponse.Content.ReadAsStringAsync();
            var preToken = JObject.Parse(preTokenJson)["token"]?.ToString();


            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", preToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    LoginModel context = new LoginModel();
                    context.empCode = empCode;
                    context.txt_pwd = txt_pwd;
                    context.app_code = app_code;
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync(loginApiUrl, content).Result;
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
                    _data = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data);

                    _data = JsonConvert.SerializeObject(result);
                    int success = Convert.ToInt32(result.Tables[0].Rows[0]["success"]);



                    //if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                    if (success == 1)
                    {
                        // Validate dataset
                        if (result == null || result.Tables.Count < 2)
                        {
                            throw new Exception("Expected 2 tables in dataset.");
                        }

                        DataTable dtPermission = result.Tables[0];
                        DataTable dtMenu = result.Tables[1];

                        // Add menu columns if not exist
                        if (!dtPermission.Columns.Contains("menu_gid"))
                            dtPermission.Columns.Add("menu_gid", typeof(string));

                        if (!dtPermission.Columns.Contains("menu_name"))
                            dtPermission.Columns.Add("menu_name", typeof(string));

                        if (!dtPermission.Columns.Contains("menu_url"))
                            dtPermission.Columns.Add("menu_url", typeof(string));

                        if (!dtPermission.Columns.Contains("menu_order"))
                            dtPermission.Columns.Add("menu_order", typeof(string));

                        if (!dtPermission.Columns.Contains("parent_menu_code"))
                            dtPermission.Columns.Add("parent_menu_code", typeof(string));

                        if (!dtPermission.Columns.Contains("icon_path"))
                            dtPermission.Columns.Add("icon_path", typeof(string));

                        var rows = dtMenu.AsEnumerable()
                         .Where(r => string.IsNullOrWhiteSpace(Convert.ToString(r["parent_menu_code"])))
                          .OrderBy(r => Convert.ToInt32(r["menu_order"]));

                        foreach (DataRow menuRow in rows)
                        {
                            DataRow newRow = dtPermission.NewRow();

                            newRow["menu_gid"] = menuRow["menu_gid"];
                            newRow["menu_code"] = menuRow["menu_code"];
                            newRow["menu_name"] = menuRow["menu_name"];
                            newRow["menu_url"] = menuRow["menu_url"];
                            newRow["menu_order"] = menuRow["menu_order"];
                            newRow["parent_menu_code"] = menuRow["parent_menu_code"];
                            newRow["icon_path"] = menuRow["icon_path"];

                            dtPermission.Rows.Add(newRow);
                        }
                        // Create lookup from menu table
                        var menuLookup = dtMenu.AsEnumerable()
                            .GroupBy(r => Convert.ToString(r["menu_code"]))
                            .ToDictionary(
                                g => g.Key,
                                g => new
                                {
                                    MenuGid = Convert.ToString(g.First()["menu_gid"]),
                                    MenuName = Convert.ToString(g.First()["menu_name"]),
                                    MenuUrl = Convert.ToString(g.First()["menu_url"]),
                                    MenuOrder = Convert.ToString(g.First()["menu_order"]),
                                    ParentMenuCode = Convert.ToString(g.First()["parent_menu_code"]),
                                    IconPath = Convert.ToString(g.First()["icon_path"])
                                }
                            );

                        // Merge menu information into permission table
                        foreach (DataRow row in dtPermission.Rows)
                        {
                            string menuCode = Convert.ToString(row["menu_code"]);

                            if (!string.IsNullOrEmpty(menuCode) &&
                                menuLookup.TryGetValue(menuCode, out var menu))
                            {
                                row["menu_gid"] = menu.MenuGid;
                                row["menu_name"] = menu.MenuName;
                                row["menu_url"] = menu.MenuUrl;
                                row["menu_order"] = menu.MenuOrder;
                                row["parent_menu_code"] = menu.ParentMenuCode;
                                row["icon_path"] = menu.IconPath;
                            }
                        }

                        // Store session values
                        if (dtPermission.Rows.Count > 0)
                        {
                            DataRow firstRow = dtPermission.Rows[0];

                            HttpContext.Session.SetString("user_role", Convert.ToString(firstRow["role_code"]));
                            HttpContext.Session.SetString("user_id", Convert.ToString(firstRow["user_id"]));
                            HttpContext.Session.SetString("user_name", Convert.ToString(firstRow["user_name"]));
                            HttpContext.Session.SetString("user_code", Convert.ToString(firstRow["user_code"]));
                            HttpContext.Session.SetString("user_email", Convert.ToString(firstRow["email"]));
                            HttpContext.Session.SetString("role_name", Convert.ToString(firstRow["role_name"]));
                        }

                        // Build Menu List
                        List<MenuModel> menuList = new List<MenuModel>();

                        foreach (DataRow row in dtPermission.Rows)
                        {
                            menuList.Add(new MenuModel
                            {
                                menu_id = Convert.ToString(row["menu_gid"]),
                                menu_code = Convert.ToString(row["menu_code"]),
                                menu_name = Convert.ToString(row["menu_name"]),
                                menu_url = Convert.ToString(row["menu_url"]),
                                menu_order = Convert.ToString(row["menu_order"]),
                                parent_menu_code = Convert.ToString(row["parent_menu_code"]),
                                icon_path = Convert.ToString(row["icon_path"]),

                                add_perm = Convert.ToString(row["add_perm"]),
                                mod_perm = Convert.ToString(row["mod_perm"]),
                                view_perm = Convert.ToString(row["view_perm"]),
                                delete_perm = Convert.ToString(row["delete_perm"]),
                                download_perm = Convert.ToString(row["download_perm"]),
                                link_perm = Convert.ToString(row["link_perm"]),
                                mail_perm = Convert.ToString(row["mail_perm"]),
                                retreq_perm = Convert.ToString(row["retreq_perm"]),
                                Approve_perm = Convert.ToString(row["Approve_perm"]),
                                Boachecklist_perm = Convert.ToString(row["Boachecklist_perm"]),
                                deny_perm = Convert.ToString(row["deny_perm"]),
                                menu_type = Convert.ToString(row["menu_type"])
                            });
                        }

                        //DataTable dtPermission = result.Tables[0];
                        //DataTable dtMenu = result.Tables[1];
                        //// Add columns if they don't exist
                        //if (!dtPermission.Columns.Contains("menu_gid"))
                        //    dtPermission.Columns.Add("menu_gid", typeof(string));

                        //if (!dtPermission.Columns.Contains("menu_name"))
                        //    dtPermission.Columns.Add("menu_name", typeof(string));

                        //if (!dtPermission.Columns.Contains("menu_url"))
                        //    dtPermission.Columns.Add("menu_url", typeof(string));

                        //if (!dtPermission.Columns.Contains("menu_order"))
                        //    dtPermission.Columns.Add("menu_order", typeof(string));

                        //if (!dtPermission.Columns.Contains("parent_menu_code"))
                        //    dtPermission.Columns.Add("parent_menu_code", typeof(string));

                        //if (!dtPermission.Columns.Contains("icon_path"))
                        //    dtPermission.Columns.Add("icon_path", typeof(string));

                        //var menuLookup = dtMenu.AsEnumerable()
                        //    .ToDictionary(
                        //        r => Convert.ToString(r["menu_code"]),
                        //        r => new
                        //        {
                        //            MenuGid = Convert.ToString(r["menu_gid"]),
                        //            MenuName = Convert.ToString(r["menu_name"]),
                        //            MenuUrl = Convert.ToString(r["menu_url"]),
                        //            MenuOrder = Convert.ToString(r["menu_order"]),
                        //            ParentMenuCode = Convert.ToString(r["parent_menu_code"]),
                        //            IconPath = Convert.ToString(r["icon_path"])
                        //        });

                        //foreach (DataRow row in dtPermission.Rows)
                        //{
                        //    string menuCode = Convert.ToString(row["menu_code"]);

                        //    if (menuLookup.TryGetValue(menuCode, out var menu))
                        //    {
                        //        row["menu_gid"] = menu.MenuGid;
                        //        row["menu_name"] = menu.MenuName;
                        //        row["menu_url"] = menu.MenuUrl;
                        //        row["menu_order"] = menu.MenuOrder;
                        //        row["parent_menu_code"] = menu.ParentMenuCode;
                        //        row["icon_path"] = menu.IconPath;
                        //    }
                        //}

                        //HttpContext.Session.SetString("user_role", Convert.ToString(result.Tables[0].Rows[0]["role_code"]));
                        //HttpContext.Session.SetString("user_id", Convert.ToString(result.Tables[0].Rows[0]["user_id"]));
                        //HttpContext.Session.SetString("user_name", Convert.ToString(result.Tables[0].Rows[0]["user_name"]));
                        //HttpContext.Session.SetString("user_code", Convert.ToString(result.Tables[0].Rows[0]["user_code"]));
                        //HttpContext.Session.SetString("user_email", Convert.ToString(result.Tables[0].Rows[0]["email"]));
                        //HttpContext.Session.SetString("role_name", Convert.ToString(result.Tables[0].Rows[0]["role_name"]));
                        //List<MenuModel> menuList = new List<MenuModel>();

                        //foreach (DataRow row in result.Tables[0].Rows)
                        //{
                        //    menuList.Add(new MenuModel
                        //    {
                        //        //menu_id = row["menu_id"].ToString(),
                        //        //menu_name = row["menu_name"].ToString(),
                        //        //menu_url = row["menu_url"].ToString(),
                        //        menu_code = row["menu_url"].ToString(),
                        //        add_perm = row["add_perm"].ToString(),
                        //        mod_perm = row["mod_perm"].ToString(),
                        //        view_perm = row["view_perm"].ToString(),
                        //        delete_perm = row["delete_perm"].ToString(),
                        //        download_perm = row["download_perm"].ToString(),
                        //        link_perm = row["link_perm"].ToString(),
                        //        mail_perm = row["mail_perm"].ToString(),
                        //        retreq_perm = row["retreq_perm"].ToString(),
                        //        Approve_perm = row["Approve_perm"].ToString(),
                        //        Boachecklist_perm = row["Boachecklist_perm"].ToString(),
                        //        deny_perm = row["deny_perm"].ToString(),
                        //        menu_type = row["menu_type"].ToString()
                        //    });
                        //}
                        var menuJson = JsonConvert.SerializeObject(menuList);
                        HttpContext.Session.SetString("UserMenus", menuJson);
                        set_Apitoken = Convert.ToString(result.Tables[0].Rows[0]["user_code"] + "_" + Convert.ToString(result.Tables[0].Rows[0]["role_code"]));

                        string id = Convert.ToString(result.Tables[0].Rows[0]["user_id"]);
                        string name = Convert.ToString(result.Tables[0].Rows[0]["user_name"]);
                        string email = Convert.ToString(result.Tables[0].Rows[0]["email"]);
                        string role = Convert.ToString(result.Tables[0].Rows[0]["role_code"]);
                        string user_code = Convert.ToString(result.Tables[0].Rows[0]["user_code"]);
                        // 🔹 Step 5: Cookie authentication for website
                        var claims = new List<Claim>
                      {
                          new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                          new Claim(ClaimTypes.Role, role.ToString()),
                          new Claim(ClaimTypes.Name, user_code.ToString()),

                      };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity)
                        );
                    }
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, set_Apitoken);
                }
                return Json(_data);
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "setFieldlist");
                return Json(ex.Message);
            }
        }


        public async Task<IActionResult> ChkLogin_Old(string empCode, string txt_pwd, string app_code)
        {
            app_code = _configuration.GetSection("AppSettings")["App_code"];
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["cumsapiurl"]) + "/ChkLogin";


            string preTokenUrl = _configuration["Appsettings:cumsapiurl"] + "/auth/generate-token";
            string loginApiUrl = _configuration["Appsettings:cumsapiurl"] + "/ChkLogin";

            using var clients = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5) // or seconds
            };

            // 🔹 Step 1: Get pre-token from API
            var preTokenResponse = await clients.PostAsync(preTokenUrl, null);
            if (!preTokenResponse.IsSuccessStatusCode)
                return Json(new { success = false, message = "Unable to get pre-token" });

            var preTokenJson = await preTokenResponse.Content.ReadAsStringAsync();
            var preToken = JObject.Parse(preTokenJson)["token"]?.ToString();


            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", preToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    LoginModel context = new LoginModel();
                    context.empCode = empCode;
                    context.txt_pwd = txt_pwd;
                    context.app_code = app_code;
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync(loginApiUrl, content).Result;
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
                    _data = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data);

                    _data = JsonConvert.SerializeObject(result);
                    int success = Convert.ToInt32(result.Tables[0].Rows[0]["success"]);

                    //if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                    if (success == 1)
                    {
                        HttpContext.Session.SetString("user_role", Convert.ToString(result.Tables[0].Rows[0]["role_code"]));
                        HttpContext.Session.SetString("user_id", Convert.ToString(result.Tables[0].Rows[0]["user_id"]));
                        HttpContext.Session.SetString("user_name", Convert.ToString(result.Tables[0].Rows[0]["user_name"]));
                        HttpContext.Session.SetString("user_code", Convert.ToString(result.Tables[0].Rows[0]["user_code"]));
                        HttpContext.Session.SetString("user_email", Convert.ToString(result.Tables[0].Rows[0]["email"]));
                        HttpContext.Session.SetString("role_name", Convert.ToString(result.Tables[0].Rows[0]["role_name"]));
                        List<MenuModel> menuList = new List<MenuModel>();

                        foreach (DataRow row in result.Tables[0].Rows)
                        {
                            menuList.Add(new MenuModel
                            {
                                menu_id = row["menu_id"].ToString(),
                                menu_name = row["menu_name"].ToString(),
                                menu_url = row["menu_url"].ToString(),
                                add_perm = row["add_perm"].ToString(),
                                mod_perm = row["mod_perm"].ToString(),
                                view_perm = row["view_perm"].ToString(),
                                delete_perm = row["delete_perm"].ToString(),
                                download_perm = row["download_perm"].ToString(),
                                link_perm = row["link_perm"].ToString(),
                                mail_perm = row["mail_perm"].ToString(),
                                retreq_perm = row["retreq_perm"].ToString(),
                                deny_perm = row["deny_perm"].ToString(),
                                menu_type = row["menu_type"].ToString()
                            });
                        }
                        var menuJson = JsonConvert.SerializeObject(menuList);
                        HttpContext.Session.SetString("UserMenus", menuJson);
                        //HttpContext.Session.SetString("mandatory_field_id", Convert.ToString(result.Tables[0].Rows[0]["mandatory_field_id"]));
                        set_Apitoken = Convert.ToString(result.Tables[0].Rows[0]["user_code"] + "_" + Convert.ToString(result.Tables[0].Rows[0]["role_code"]));

                        string id = Convert.ToString(result.Tables[0].Rows[0]["user_id"]);
                        string name = Convert.ToString(result.Tables[0].Rows[0]["user_name"]);
                        string email = Convert.ToString(result.Tables[0].Rows[0]["email"]);
                        string role = Convert.ToString(result.Tables[0].Rows[0]["role_code"]);
                        string user_code = Convert.ToString(result.Tables[0].Rows[0]["user_code"]);
                        // 🔹 Step 5: Cookie authentication for website
                        var claims = new List<Claim>
                             {
                                 new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                                 new Claim(ClaimTypes.Role, role.ToString()),
                                 new Claim(ClaimTypes.Name, user_code.ToString()),
                                 
                             };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity)
                        );
                    }
                    ApiTokenRefreshMiddleware.TokenUpdate(HttpContext, response, set_Apitoken);
                }
                return Json(_data);
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "setFieldlist");
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ChangePassword([FromBody] LoginModel mymodel)
        {
            var apiUrl = _configuration.GetSection("Appsettings")["apiurl"] + "/ChangePassword";

            try
            {
                using var client = new HttpClient();
                client.Timeout = Timeout.InfiniteTimeSpan;
                APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                string token = Request.Cookies[APIcookieName];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonContent = new StringContent(JsonConvert.SerializeObject(mymodel), Encoding.UTF8, "application/json");
                var response = client.PostAsync(apiUrl, jsonContent).Result;

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
                {
                    return Json(new { success = false, message = $"API call failed with status: {response.StatusCode}" });
                }

                var rawJson = response.Content.ReadAsStringAsync().Result;
                var nestedJson = JsonConvert.DeserializeObject<string>(rawJson);
                var parsedResult = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(nestedJson);

                var message = parsedResult?["Table"]?.FirstOrDefault()?["result"] ?? "Unknown response";
                var isSuccess = message.ToLower().Contains("success");

                return Json(new { success = isSuccess, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult generateOTP(String empEmail)
        {
            urlstring = Convert.ToString(_configuration.GetSection("Appsettings")["apiurl"]) + "/getOTP"; ;
            DataSet result = new DataSet();
            string post_data = "";
            try
            {
                using (var client = new HttpClient())
                {
                    // client.BaseAddress = new Uri(urlstring);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    APIcookieName = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
                    string token = Request.Cookies[APIcookieName];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    LoginModel context = new LoginModel();
                    context.empEmail = empEmail;
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(context), UTF8Encoding.UTF8, "application/json");
                    var response = client.PostAsync(urlstring, content).Result;
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
                    _data = JsonConvert.DeserializeObject<string>(post_data);
                    result = JsonConvert.DeserializeObject<DataSet>(_data);
                    _data = JsonConvert.SerializeObject(result.Tables[0]);

                }
                return Json(_data);
            }
            catch (Exception ex)
            {
                CommonController objcom = new CommonController(_configuration);
                objcom.errorlog(ex.Message, "setFieldlist");
                return Json(ex.Message);
            }
        }

        public async Task<IActionResult> Logout()
        {
            set_Apitoken = "APItoken-" + User.FindFirst(ClaimTypes.Name)?.Value.ToString() + "_" + User.FindFirst(ClaimTypes.Role)?.Value.ToString();
            Response.Cookies.Delete(set_Apitoken);
            return RedirectToAction("Login", "Login");
        }
    }
}

   

