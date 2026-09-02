using System;
using Microsoft.AspNetCore.Mvc;

namespace IPOWeb.Attributes
{
    /// <summary>
    /// Apply this attribute to controller actions or Razor Page handlers to enable audit logging for that action.
    /// The implementation is provided by AuditFilter via DI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuditAttribute : TypeFilterAttribute
    {
        public AuditAttribute(string businessActionName = "") : base(typeof(IPOWeb.Filters.AuditFilter))
        {
            Arguments = new object[] { businessActionName };
        }
    }
}
