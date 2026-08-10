using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

public class AuditLogListQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    [MaxLength(200, ErrorMessage = "Search term is too long.")]
    public string? SearchTerm { get; set; }

    [MaxLength(100, ErrorMessage = "Action filter is too long.")]
    public string? ActionFilter { get; set; }

    [MaxLength(100, ErrorMessage = "Table name filter is too long.")]
    public string? TableNameFilter { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Valid UserId is required.")]
    public int? UserIdFilter { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
