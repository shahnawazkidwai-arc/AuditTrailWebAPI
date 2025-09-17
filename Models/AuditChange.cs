namespace audit_trail_.NET_Core_Web_API.Models
{
    public class AuditChange
    {
       
        public string Path { get; set; } = default!;
        public string? Before { get; set; }
        public string? After { get; set; }
    }
}
