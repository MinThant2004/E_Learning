using System;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

public class CreateAuditLogRequest
{
    public int UserId { get; set; }
    public string Action { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }

    /// <summary>
    /// Optional field-level before/after changes for Update actions.
    /// Serialized as JSON into AuditLog.Changes.
    /// </summary>
    public List<AuditLogChangeDto>? Changes { get; set; }
}
