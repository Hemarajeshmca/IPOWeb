using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IPOWeb.Middlewares
{
    /// <summary>
    /// Middleware to ensure each request has a correlation id and a recorded start time and IP.
    /// The values are stored in HttpContext.Items so filters can read them later.
    /// </summary>
    public class AuditCorrelationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditCorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Correlation Id: prefer incoming header, otherwise generate
            const string headerName = "X-Correlation-ID";
            string correlationId = context.Request.Headers.ContainsKey(headerName)
                ? context.Request.Headers[headerName].ToString()!
                : Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers[headerName] = correlationId;

            // Record request start time
            context.Items["AuditStartTime"] = DateTimeOffset.UtcNow;

            // Record remote IP address (best-effort)
           // context.Items["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var ipAddress = context.Connection.RemoteIpAddress;

            if (ipAddress != null)
            {
                // Maps ::1 to 127.0.0.1
                if (ipAddress.IsIPv4MappedToIPv6 || ipAddress.ToString() == "::1")
                {
                    ipAddress = ipAddress.MapToIPv4();
                }
            }

            context.Items["RemoteIpAddress"] = ipAddress?.ToString() ?? string.Empty;

            await _next(context);
        }
    }
}
