using audit_trail_.NET_Core_Web_API.Models;
using System.Collections.Concurrent;

namespace audit_trail_.NET_Core_Web_API.Repositories
{
    public class InMemoryAuditRepository : IAuditRepository
    {
        private readonly ConcurrentDictionary<Guid, AuditRecord> _store = new();

        public Task SaveAsync(AuditRecord record, CancellationToken cancellationToken = default)
        {
            _store[record.Id] = record;
            return Task.CompletedTask;
        }

        public Task<AuditRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var rec);
            return Task.FromResult(rec);
        }

        public Task<IReadOnlyList<AuditRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AuditRecord>>(_store.Values.OrderByDescending(r => r.Timestamp).ToList());
        }
    }
}
