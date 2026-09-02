using System;
using IPOWeb.Middlewares;
using IPOWeb.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IPOWeb.Extensions
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddAuditLogging(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            // Register HttpClient for sending audit entries to remote API
            services.AddHttpClient();
            // Use ApiAuditStore by default to send framed model to an external API
            services.AddScoped<IAuditLogger, ApiAuditStore>();
            return services;
        }

        public static IApplicationBuilder UseAuditCorrelation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuditCorrelationMiddleware>();
        }
    }
}
