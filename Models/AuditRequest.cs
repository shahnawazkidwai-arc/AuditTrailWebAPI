using System.Text.Json;

namespace audit_trail_.NET_Core_Web_API.Models
{
    public class AuditRequest
    {
        
        public JsonElement? Before { get; set; }    
        public JsonElement? After { get; set; }     

        public string EntityName { get; set; } = default!;
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = default!;
        public DateTime? Timestamp { get; set; }    
    }
}
