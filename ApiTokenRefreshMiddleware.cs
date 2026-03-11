using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace IPOWeb
{
    public class ApiTokenRefreshMiddleware
    {
         
        public static async void TokenUpdate(HttpContext context, HttpResponseMessage apiResponse,string APIcookieName)
        { 
            //UserProvider user = new UserProvider();
            //string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //string role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (apiResponse.Headers.TryGetValues("X-New-Token", out var newTokenHeader))
            { 
                var newToken = newTokenHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(newToken))
                {
                    
                    if (APIcookieName == null) return;

                    //string APIcookieName = "APItoken-" + uid + "_" + urole;
                    //context.Response.Cookies.Append("API_TOKEN", newToken, new CookieOptions
                    context.Response.Cookies.Append("APItoken-"+APIcookieName, newToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // set true in production
                        SameSite = SameSiteMode.Strict
                        //Expires = DateTime.UtcNow.AddMinutes(15)
                    });
                    
                    // optional: store in HttpContext.Items if you want to use in controller
                    context.Items["Userinfo"] = context;
                }
            }
        }
    }
}
