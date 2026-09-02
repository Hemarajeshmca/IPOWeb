using System.Threading.Tasks;
using IPOWeb.Models;

namespace IPOWeb.Services
{
    public interface IAuditLogger
    {
        Task LogAsync(AuditLogEntryModel entry);
    }
}
