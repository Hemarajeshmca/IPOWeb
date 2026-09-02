namespace IPOWeb.Models
{
    public class AuditLoggingOptions
    {
        // Master switch
        public bool Enabled { get; set; } = true;

        // When true, only actions matching IncludeActions will be logged
        public bool UseWhitelist { get; set; } = false;

        // Patterns to include (substring match)
        public System.Collections.Generic.List<string> IncludeActions { get; set; } = new System.Collections.Generic.List<string>();

        // Patterns to exclude (substring match)
        public System.Collections.Generic.List<string> ExcludeActions { get; set; } = new System.Collections.Generic.List<string>();

        // Case sensitivity for pattern matching
        public bool CaseSensitive { get; set; } = false;
    }
}