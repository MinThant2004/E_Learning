using System;

namespace ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

public class AuditLogListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? ActionFilter { get; set; }
    public string? TableNameFilter { get; set; }
    public int? UserIdFilter { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
