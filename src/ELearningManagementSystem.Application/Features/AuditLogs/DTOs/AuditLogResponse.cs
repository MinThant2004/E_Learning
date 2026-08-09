using System;

namespace ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

public class AuditLogResponse
{
    public int AuditLogId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
}
