using System;
using ELearningManagementSystem.Application.Common;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class RevenueReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "ApprovedRevenue";
    public bool SortDescending { get; set; } = true;
}

public class RevenueReportDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal ExamFee { get; set; }

    public int TotalPayments { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedRevenue { get; set; }
    public decimal RejectedAmount { get; set; }
}

public class RevenueReportSummaryDto
{
    public decimal TotalApprovedRevenue { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalApprovedCount { get; set; }
    public int TotalPendingCount { get; set; }
}

public class RevenueReportResponse
{
    public PagedResult<RevenueReportDto> Data { get; set; } = null!;
    public RevenueReportSummaryDto Summary { get; set; } = new();
}
