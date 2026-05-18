namespace MaverickBank.Core.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}