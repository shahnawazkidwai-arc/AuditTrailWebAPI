namespace audit_trail_.NET_Core_Web_API.Models
{
    public class AuditRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EntityName { get; set; } = default!;
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public List<AuditChange> Changes { get; set; } = new List<AuditChange>();
    }
}
