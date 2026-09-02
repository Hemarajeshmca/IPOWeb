using System;

namespace IPOWeb.Models
{
    /// <summary>
    /// Represents an audit / action log entry for tracking controller actions and API calls.
    /// </summary>
    public class AuditLogEntryModel
    {
        // Optional primary key if persisted with EF/Core
        public int Id { get; set; }

        // User information
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // Application and routing
        public string ApplicationName { get; set; } = string.Empty; // e.g. "IPO_Web" or "IPO_API"
        public string ControllerName { get; set; } = string.Empty;
        public string ActionMethodName { get; set; } = string.Empty;
        public string BusinessActionName { get; set; } = string.Empty; // optional business-level name

        // HTTP / request details
        public string HttpMethod { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string ActionParametersJson { get; set; } = string.Empty; // JSON of parameters/values
        public string IpAddress { get; set; } = string.Empty;

        // Outcome and diagnostics
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public bool IsSuccess { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty; // populated when IsSuccess == false
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public int? ResponseStatusCode { get; set; }
    }
}
