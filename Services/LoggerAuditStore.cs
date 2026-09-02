using System.Text.Json;
using System.Threading.Tasks;
using IPOWeb.Models;
using Microsoft.Extensions.Logging;

namespace IPOWeb.Services
{
    /// <summary>
    /// Default audit logger that writes audit entries to the application's ILogger.
    /// Replace or extend this to persist to a database or external store.
    /// </summary>
    public class LoggerAuditStore : IAuditLogger
    {
        private readonly ILogger<LoggerAuditStore> _logger;

        public LoggerAuditStore(ILogger<LoggerAuditStore> logger)
        {
            _logger = logger;
        }

        public Task LogAsync(AuditLogEntryModel entry)
        {
            // Serialize the important parts to a compact JSON for structured logging
            var payload = new
            {
                entry.CorrelationId,
                entry.Timestamp,
                entry.ApplicationName,
                entry.ControllerName,
                entry.ActionMethodName,
                entry.BusinessActionName,
                entry.UserId,
                entry.UserName,
                entry.HttpMethod,
                entry.RequestUrl,
                Parameters = TryParseJson(entry.ActionParametersJson),
                entry.IpAddress,
                entry.IsSuccess,
                entry.ResponseStatusCode,
                Exception = entry.ExceptionMessage
            };

            _logger.LogInformation("AuditLog: {Audit}", JsonSerializer.Serialize(payload));

            return Task.CompletedTask;
        }

        private static object? TryParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch
            {
                return json;
            }
        }
    }
}
