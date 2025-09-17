using audit_trail_.NET_Core_Web_API.Models;

namespace audit_trail_.NET_Core_Web_API.Services
{
    public interface IAuditService
    {
        Task<AuditRecord> CreateAuditAsync(AuditRequest request, CancellationToken cancellationToken = default);
    }
}
