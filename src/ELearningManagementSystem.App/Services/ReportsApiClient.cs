using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class ReportsApiClient
{
    private readonly HttpClient _httpClient;

    public ReportsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 1. Enrollment Report Client Methods
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<(bool Success, PagedResult<ClientEnrollmentReportDto>? Data, string? Error)> GetEnrollmentReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        string? statusFilter = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "EnrollDate",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/enrollments", startDate, endDate, searchTerm, page, pageSize, sortBy, sortDescending);
        if (!string.IsNullOrWhiteSpace(statusFilter))
            url += $"&statusFilter={Uri.EscapeDataString(statusFilter)}";

        return await ExecuteGetAsync<PagedResult<ClientEnrollmentReportDto>>(url);
    }

    public string GetEnrollmentReportExportUrl(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        string? statusFilter = null,
        string sortBy = "EnrollDate",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/enrollments/export", startDate, endDate, searchTerm, 1, 5000, sortBy, sortDescending);
        if (!string.IsNullOrWhiteSpace(statusFilter))
            url += $"&statusFilter={Uri.EscapeDataString(statusFilter)}";

        return url;
    }

    public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadEnrollmentReportCsvAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        string? statusFilter = null,
        string sortBy = "EnrollDate",
        bool sortDescending = true)
    {
        var url = GetEnrollmentReportExportUrl(startDate, endDate, searchTerm, statusFilter, sortBy, sortDescending);
        return await DownloadCsvFileAsync(url, "enrollment_report");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 2. Course Performance Report Client Methods
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<(bool Success, PagedResult<ClientCoursePerformanceReportDto>? Data, string? Error)> GetCoursePerformanceReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "TotalEnrollments",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/course-performance", startDate, endDate, searchTerm, page, pageSize, sortBy, sortDescending);
        return await ExecuteGetAsync<PagedResult<ClientCoursePerformanceReportDto>>(url);
    }

    public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadCoursePerformanceReportCsvAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        string sortBy = "TotalEnrollments",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/course-performance/export", startDate, endDate, searchTerm, 1, 5000, sortBy, sortDescending);
        return await DownloadCsvFileAsync(url, "course_performance_report");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 3. Quiz Performance Report Client Methods
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<(bool Success, PagedResult<ClientQuizPerformanceReportDto>? Data, string? Error)> GetQuizPerformanceReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "TotalAttempts",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/quiz-performance", startDate, endDate, searchTerm, page, pageSize, sortBy, sortDescending);
        return await ExecuteGetAsync<PagedResult<ClientQuizPerformanceReportDto>>(url);
    }

    public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadQuizPerformanceReportCsvAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        string sortBy = "TotalAttempts",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/quiz-performance/export", startDate, endDate, searchTerm, 1, 5000, sortBy, sortDescending);
        return await DownloadCsvFileAsync(url, "quiz_performance_report");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 4. Audit Activity Report Client Methods
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<(bool Success, PagedResult<ClientAuditLogResponse>? Data, string? Error)> GetAuditActivityReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        int? userIdFilter = null,
        string? actionFilter = null,
        string? tableNameFilter = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "CreatedAt",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/audit-activity", startDate, endDate, searchTerm, page, pageSize, sortBy, sortDescending);
        if (userIdFilter.HasValue)
            url += $"&userIdFilter={userIdFilter.Value}";
        if (!string.IsNullOrWhiteSpace(actionFilter))
            url += $"&actionFilter={Uri.EscapeDataString(actionFilter)}";
        if (!string.IsNullOrWhiteSpace(tableNameFilter))
            url += $"&tableNameFilter={Uri.EscapeDataString(tableNameFilter)}";

        return await ExecuteGetAsync<PagedResult<ClientAuditLogResponse>>(url);
    }

    public async Task<(bool Success, ClientAuditActivityFilterOptionsDto? Data, string? Error)> GetAuditActivityFilterOptionsAsync()
    {
        return await ExecuteGetAsync<ClientAuditActivityFilterOptionsDto>("api/reports/audit-activity/options");
    }

    public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadAuditActivityReportCsvAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        int? userIdFilter = null,
        string? actionFilter = null,
        string? tableNameFilter = null,
        string sortBy = "CreatedAt",
        bool sortDescending = true)
    {
        var url = BuildUrl("api/reports/audit-activity/export", startDate, endDate, searchTerm, 1, 5000, sortBy, sortDescending);
        if (userIdFilter.HasValue)
            url += $"&userIdFilter={userIdFilter.Value}";
        if (!string.IsNullOrWhiteSpace(actionFilter))
            url += $"&actionFilter={Uri.EscapeDataString(actionFilter)}";
        if (!string.IsNullOrWhiteSpace(tableNameFilter))
            url += $"&tableNameFilter={Uri.EscapeDataString(tableNameFilter)}";

        return await DownloadCsvFileAsync(url, "audit_activity_report");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Internal Helper Methods
    // ─────────────────────────────────────────────────────────────────────────────
    private static string BuildUrl(string basePath, DateTime? startDate, DateTime? endDate, string? searchTerm, int page, int pageSize, string sortBy, bool sortDescending)
    {
        var sb = new StringBuilder($"{basePath}?page={page}&pageSize={pageSize}");

        if (startDate.HasValue)
            sb.Append($"&startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            sb.Append($"&endDate={endDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(searchTerm.Trim())}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            sb.Append($"&sortBy={Uri.EscapeDataString(sortBy)}");

        sb.Append($"&sortDescending={sortDescending}");

        return sb.ToString();
    }

    private async Task<(bool Success, T? Data, string? Error)> ExecuteGetAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return (true, data, null);
            }

            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, default, error);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    private async Task<(bool Success, byte[]? FileBytes, string? FileName, string? Error)> DownloadCsvFileAsync(string url, string defaultName)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                               ?? $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return (true, bytes, fileName, null);
            }

            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, null, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Client DTOs
// ─────────────────────────────────────────────────────────────────────────────
public class ClientEnrollmentReportDto
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string CompletionStatus { get; set; } = string.Empty;
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
    public double ProgressPercentage { get; set; }
}

public class ClientCoursePerformanceReportDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool CourseStatus { get; set; }
    public int TotalEnrollments { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int NotStartedCount { get; set; }
    public double CompletionRatePercentage { get; set; }
    public int TotalLessonsCount { get; set; }
    public int TotalQuizzesCount { get; set; }
}

public class ClientQuizPerformanceReportDto
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal PassingScore { get; set; }
    public int TotalAttempts { get; set; }
    public int PassedAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double AverageScore { get; set; }
    public double PassRatePercentage { get; set; }
    public double FailRatePercentage { get; set; }
    public double HighestScore { get; set; }
}

public class ClientAuditActivityFilterOptionsDto
{
    public List<ClientReportUserDto> Users { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public List<string> TableNames { get; set; } = new();
}

public class ClientReportUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
