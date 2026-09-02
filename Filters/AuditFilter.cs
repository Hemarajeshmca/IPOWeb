using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using IPOWeb.Models;
using Microsoft.Extensions.Options;
using IPOWeb.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IPOWeb.Filters
{
    /// <summary>
    /// Filter that captures audit information for controller actions and Razor Page handlers.
    /// Register this filter through the AuditAttribute (TypeFilter) so it can use DI.
    /// </summary>
    public class AuditFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AuditFilter> _logger;
        private readonly string _businessActionName;
        private readonly AuditLoggingOptions _options;

        // Note: TypeFilter passes attribute arguments first, then DI-resolved services.
        public AuditFilter(string businessActionName, IAuditLogger auditLogger, ILogger<AuditFilter> logger, IOptions<AuditLoggingOptions> options)
        {
            _businessActionName = businessActionName ?? string.Empty;
            _auditLogger = auditLogger;
            _logger = logger;
            _options = options?.Value ?? new AuditLoggingOptions();
        }

        // Parameterless DI constructor for global registration (no attribute argument supplied)
        public AuditFilter(IAuditLogger auditLogger, ILogger<AuditFilter> logger, IOptions<AuditLoggingOptions> options)
        {
            _businessActionName = string.Empty;
            _auditLogger = auditLogger;
            _logger = logger;
            _options = options?.Value ?? new AuditLoggingOptions();
        }

        private bool ShouldLog(string controllerOrPage, string actionOrHandler, string businessAction)
        {
            try
            {
                if (_options == null || !_options.Enabled)
                {
                    return false;
                }

                // Determine a candidate name to check: prefer explicit business action name, then controller.action
                var candidate = !string.IsNullOrWhiteSpace(businessAction) ? businessAction : (controllerOrPage + "." + actionOrHandler).Trim('.');
                if (string.IsNullOrWhiteSpace(candidate)) return false;

                if (!_options.CaseSensitive)
                {
                    candidate = candidate.ToLowerInvariant();
                }

                bool Matches(string pattern)
                {
                    if (string.IsNullOrWhiteSpace(pattern)) return false;
                    var p = _options.CaseSensitive ? pattern : pattern.ToLowerInvariant();
                    return candidate.Contains(p);
                }

                // If using whitelist, only log when any include pattern matches
                if (_options.UseWhitelist)
                {
                    if (_options.IncludeActions == null || !_options.IncludeActions.Any()) return false;
                    return _options.IncludeActions.Any(Matches);
                }

                // Otherwise use blacklist semantics: skip when any exclude matches
                if (_options.ExcludeActions != null && _options.ExcludeActions.Any(Matches)) return false;

                // If IncludeActions provided and not using whitelist, treat them as additional must-log patterns
                if (_options.IncludeActions != null && _options.IncludeActions.Any())
                {
                    return _options.IncludeActions.Any(Matches);
                }
                // Default: log
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating audit logging rules; defaulting to not logging");
                return false;
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var start = DateTimeOffset.Now;
            var correlationId = http.Items.ContainsKey("CorrelationId") ? http.Items["CorrelationId"]?.ToString() : Guid.NewGuid().ToString();

            string userId = GetUserId(http.User);
            string userName = http.User?.Identity?.Name ?? string.Empty;

            string controllerName = string.Empty;
            string actionName = string.Empty;
            if (context.ActionDescriptor is ControllerActionDescriptor cad)
            {
                controllerName = cad.ControllerName;
                actionName = cad.ActionName;
            }

            string requestUrl = http.Request?.Path + http.Request?.QueryString;
            string httpMethod = http.Request?.Method ?? string.Empty;
            string ip = http.Items.ContainsKey("RemoteIpAddress") ? http.Items["RemoteIpAddress"]?.ToString() ?? string.Empty : http.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            string parametersJson = SerializeActionArguments(context.ActionArguments);

            ActionExecutedContext executedContext = null;
            Exception? exception = null;

            // Decide whether to log BEFORE executing action so we don't return from finally
            bool shouldLog = ShouldLog(controllerName, actionName, _businessActionName);

            try
            {
                var executed = await next();
                executedContext = executed;
                exception = executed.Exception;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                if (!shouldLog)
                {
                    // nothing to do
                }
                else
                {
                    var entry = new AuditLogEntryModel
                    {
                        Timestamp = start,
                        CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                        UserId = userId,
                        UserName = userName,
                        ApplicationName = "IPO_Web",
                        ControllerName = controllerName,
                        ActionMethodName = actionName,
                        BusinessActionName = _businessActionName ?? string.Empty,
                        HttpMethod = httpMethod,
                        RequestUrl = requestUrl ?? string.Empty,
                        ActionParametersJson = parametersJson,
                        IpAddress = ip,
                        IsSuccess = exception == null && (http.Response?.StatusCode < 400),
                        ExceptionMessage = exception?.ToString() ?? string.Empty,
                        ResponseStatusCode = http.Response?.StatusCode
                    };

                    try
                    {
                        await _auditLogger.LogAsync(entry);
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogError(logEx, "Failed to write audit entry");
                    }
                }
            }
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var http = context.HttpContext;
            var start = http.Items.ContainsKey("AuditStartTime") ? (DateTimeOffset)http.Items["AuditStartTime"] : DateTimeOffset.UtcNow;
            var correlationId = http.Items.ContainsKey("CorrelationId") ? http.Items["CorrelationId"]?.ToString() : Guid.NewGuid().ToString();

            string userId = GetUserId(http.User);
            string userName = http.User?.Identity?.Name ?? string.Empty;

            string pageName = context.ActionDescriptor?.DisplayName ?? string.Empty;
            string handlerName = context.HandlerMethod?.Name ?? string.Empty;

            string requestUrl = http.Request?.Path + http.Request?.QueryString;
            string httpMethod = http.Request?.Method ?? string.Empty;
            string ip = http.Items.ContainsKey("RemoteIpAddress") ? http.Items["RemoteIpAddress"]?.ToString() ?? string.Empty : http.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            string parametersJson = SerializeActionArguments(context.HandlerArguments);

            PageHandlerExecutedContext executedContext = null;
            Exception? exception = null;

            // Decide whether to log BEFORE executing handler to avoid returning from finally
            bool shouldLog = ShouldLog(pageName, handlerName, _businessActionName);

            try
            {
                var executed = await next();
                executedContext = executed;
                exception = executed.Exception;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                if (!shouldLog)
                {
                    // nothing to do
                }
                else
                {
                    var entry = new AuditLogEntryModel
                    {
                        Timestamp = start,
                        CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                        UserId = userId,
                        UserName = userName,
                        ApplicationName = "IPO_Web",
                        ControllerName = pageName,
                        ActionMethodName = handlerName,
                        BusinessActionName = _businessActionName ?? string.Empty,
                        HttpMethod = httpMethod,
                        RequestUrl = requestUrl ?? string.Empty,
                        ActionParametersJson = parametersJson,
                        IpAddress = ip,
                        IsSuccess = exception == null && (http.Response?.StatusCode < 400),
                        ExceptionMessage = exception?.ToString() ?? string.Empty,
                        ResponseStatusCode = http.Response?.StatusCode
                    };

                    try
                    {
                        await _auditLogger.LogAsync(entry);
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogError(logEx, "Failed to write audit entry");
                    }
                }
            }
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            // not used
            return Task.CompletedTask;
        }

        private static string GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) return string.Empty;
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value
                   ?? user.Identity?.Name
                   ?? string.Empty;
        }

        private static string SerializeActionArguments(System.Collections.Generic.IDictionary<string, object?> args)
        {
            if (args == null || args.Count == 0) return string.Empty;
            try
            {
                return JsonSerializer.Serialize(args);
            }
            catch
            {
                return string.Join(',', args.Select(kv => kv.Key + ":" + (kv.Value?.ToString() ?? "null")));
            }
        }
    }
}
