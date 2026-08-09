using System;

namespace ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

public class CreateAuditLogRequest
{
    public int UserId { get; set; }
    public string Action { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }
}
