using audit_trail_.NET_Core_Web_API.Models;

namespace audit_trail_.NET_Core_Web_API.Repositories
{
    public interface IAuditRepository
    {
        Task SaveAsync(AuditRecord record, CancellationToken cancellationToken = default);
        Task<AuditRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
