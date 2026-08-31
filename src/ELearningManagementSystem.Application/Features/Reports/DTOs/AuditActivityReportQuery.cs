using System;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class AuditActivityReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UserIdFilter { get; set; }
    public string? ActionFilter { get; set; }
    public string? TableNameFilter { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
