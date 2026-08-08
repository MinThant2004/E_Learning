namespace ELearningManagementSystem.Domain.Entities;

public partial class AuditLog
{
    public int AuditLogId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
