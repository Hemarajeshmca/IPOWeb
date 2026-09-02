using System.Net;
using Microsoft.AspNetCore.Http;

namespace IPOWeb.Services
{
    public interface IClientInfoService
    {
        string GetClientIp();
        string GetUserAgent();
    }
    public class ClientInfoService : IClientInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return string.Empty;

            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }



        public string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return string.Empty;

            return context.Request.Headers["User-Agent"].ToString();
        }
    }
}
