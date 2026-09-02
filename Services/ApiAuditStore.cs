using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
//using IPOWeb.Middleware;
using IPOWeb.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IPOWeb.Services
{
    /// <summary>
    /// Sends audit entries to a remote HTTP API endpoint. Configure the endpoint with "Audit:ApiUrl" in appsettings.
    /// </summary>
    public class ApiAuditStore : IAuditLogger
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly ILogger<ApiAuditStore> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

       

        public ApiAuditStore(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiAuditStore> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _apiUrl = configuration["Audit:ApiUrl"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_apiUrl))
            {
                _logger.LogWarning("Audit:ApiUrl configuration is not set. Audit entries will not be sent.");
            }
        }

        public async Task LogAsync(AuditLogEntryModel entry)
        {
            // Determine the target URL: prefer configured Audit:ApiUrl, otherwise fall back to Appsettings:apiurl + "/Audit"
            string targetUrl = _apiUrl;
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                var baseApi = _configuration.GetSection("Appsettings")["apiurl"];
                if (!string.IsNullOrWhiteSpace(baseApi))
                {
                    targetUrl = baseApi.TrimEnd('/') + "/auditLog";
                }
            }

            if (string.IsNullOrWhiteSpace(targetUrl)) return;

            try
            {
                var client = _httpClientFactory.CreateClient();
                // match controller pattern: infinite timeout
                client.Timeout = Timeout.InfiniteTimeSpan;

                var httpContext = _httpContextAccessor?.HttpContext;
                string APIcookieName = null;

                // Attach bearer token from cookie pattern used in controllers (APItoken-{username}_{role})
                try
                {
                    if (httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        var userName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                        var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(role))
                        {
                            APIcookieName = "APItoken-" + userName + "_" + role;
                            if (httpContext.Request.Cookies.TryGetValue(APIcookieName, out var token) && !string.IsNullOrWhiteSpace(token))
                            {
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Unable to attach bearer token from cookie for audit API call.");
                }

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(entry, options);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(targetUrl, content).ConfigureAwait(false);

                // Allow ApiTokenRefreshMiddleware to update cookie if header present
                try
                {
                    if (httpContext != null)
                    {
                        ApiTokenRefreshMiddleware.TokenUpdate(httpContext, resp, APIcookieName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "ApiTokenRefreshMiddleware failed while processing audit API response.");
                }

                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        if (httpContext != null && !string.IsNullOrEmpty(APIcookieName))
                        {
                            httpContext.Response.Cookies.Delete(APIcookieName);
                        }
                    }
                    catch { }

                    _logger.LogWarning("Audit API returned Unauthorized for CorrelationId={CorrelationId}", entry.CorrelationId);
                    return;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Audit API returned {Status} when posting audit entry. CorrelationId={CorrelationId}", resp.StatusCode, entry.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send audit entry to API. CorrelationId={CorrelationId}", entry.CorrelationId);
            }
        }
    }
}
