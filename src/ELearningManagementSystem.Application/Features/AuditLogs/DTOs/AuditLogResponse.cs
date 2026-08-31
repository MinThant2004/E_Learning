using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Human-readable name of the target record (e.g. category name, course title, user full name),
    /// resolved by table + record id. Null when the record no longer exists or the table is unknown.
    /// </summary>
    public string? RecordName { get; set; }

    /// <summary>
    /// Field-level before/after changes for Update actions (null when not captured).
    /// </summary>
    public List<AuditLogChangeDto>? Changes { get; set; }
}

public class AuditLogChangeDto
{
    public string Field { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
