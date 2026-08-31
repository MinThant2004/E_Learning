using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Reports.DTOs;
using ELearningManagementSystem.Application.Features.Reports.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 1. Enrollment Report Endpoints
    // ─────────────────────────────────────────────────────────────────────────────
    [HttpGet("enrollments")]
    [Authorize(Policy = "Permission:EnrollmentHistory.Read")]
    public async Task<IActionResult> GetEnrollmentReport([FromQuery] EnrollmentReportQuery query)
    {
        var result = await _reportService.GetEnrollmentReportAsync(query);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.StartsWith("ValidationError"))
                return BadRequest(new { Error = result.Error });

            return BadRequest(new { Error = result.Error ?? "Failed to fetch enrollment report." });
        }

        return Ok(result.Value);
    }

    [HttpGet("enrollments/export")]
    [Authorize(Policy = "Permission:EnrollmentHistory.Read")]
    public async Task<IActionResult> ExportEnrollmentReportCsv([FromQuery] EnrollmentReportQuery query)
    {
        var result = await _reportService.ExportEnrollmentReportCsvAsync(query);
        if (result.IsFailure || result.Value == null)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to export enrollment report." });
        }

        return File(result.Value, "text/csv", $"enrollment_report_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 2. Course Performance Report Endpoints
    // ─────────────────────────────────────────────────────────────────────────────
    [HttpGet("course-performance")]
    [Authorize(Policy = "Permission:CoursePerformance.Read")]
    public async Task<IActionResult> GetCoursePerformanceReport([FromQuery] CoursePerformanceReportQuery query)
    {
        var result = await _reportService.GetCoursePerformanceReportAsync(query);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.StartsWith("ValidationError"))
                return BadRequest(new { Error = result.Error });

            return BadRequest(new { Error = result.Error ?? "Failed to fetch course performance report." });
        }

        return Ok(result.Value);
    }

    [HttpGet("course-performance/export")]
    [Authorize(Policy = "Permission:CoursePerformance.Read")]
    public async Task<IActionResult> ExportCoursePerformanceReportCsv([FromQuery] CoursePerformanceReportQuery query)
    {
        var result = await _reportService.ExportCoursePerformanceReportCsvAsync(query);
        if (result.IsFailure || result.Value == null)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to export course performance report." });
        }

        return File(result.Value, "text/csv", $"course_performance_report_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 3. Quiz Performance Report Endpoints
    // ─────────────────────────────────────────────────────────────────────────────
    [HttpGet("quiz-performance")]
    [Authorize(Policy = "Permission:QuizPerformance.Read")]
    public async Task<IActionResult> GetQuizPerformanceReport([FromQuery] QuizPerformanceReportQuery query)
    {
        var result = await _reportService.GetQuizPerformanceReportAsync(query);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.StartsWith("ValidationError"))
                return BadRequest(new { Error = result.Error });

            return BadRequest(new { Error = result.Error ?? "Failed to fetch quiz performance report." });
        }

        return Ok(result.Value);
    }

    [HttpGet("quiz-performance/export")]
    [Authorize(Policy = "Permission:QuizPerformance.Read")]
    public async Task<IActionResult> ExportQuizPerformanceReportCsv([FromQuery] QuizPerformanceReportQuery query)
    {
        var result = await _reportService.ExportQuizPerformanceReportCsvAsync(query);
        if (result.IsFailure || result.Value == null)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to export quiz performance report." });
        }

        return File(result.Value, "text/csv", $"quiz_performance_report_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 4. Audit Activity Report Endpoints
    // ─────────────────────────────────────────────────────────────────────────────
    [HttpGet("audit-activity")]
    [Authorize(Policy = "Permission:AuditLog.Read")]
    public async Task<IActionResult> GetAuditActivityReport([FromQuery] AuditActivityReportQuery query)
    {
        var result = await _reportService.GetAuditActivityReportAsync(query);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.StartsWith("ValidationError"))
                return BadRequest(new { Error = result.Error });

            return BadRequest(new { Error = result.Error ?? "Failed to fetch audit activity report." });
        }

        return Ok(result.Value);
    }

    [HttpGet("audit-activity/export")]
    [Authorize(Policy = "Permission:AuditLog.Read")]
    public async Task<IActionResult> ExportAuditActivityReportCsv([FromQuery] AuditActivityReportQuery query)
    {
        var result = await _reportService.ExportAuditActivityReportCsvAsync(query);
        if (result.IsFailure || result.Value == null)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to export audit activity report." });
        }

        return File(result.Value, "text/csv", $"audit_activity_report_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("audit-activity/options")]
    [Authorize(Policy = "Permission:AuditLog.Read")]
    public async Task<IActionResult> GetAuditActivityFilterOptions()
    {
        var result = await _reportService.GetAuditActivityFilterOptionsAsync();
        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to fetch filter options." });
        }

        return Ok(result.Value);
    }
}
