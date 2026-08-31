using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Reports.DTOs;

namespace ELearningManagementSystem.Application.Features.Reports.Services;

public interface IReportService
{
    Task<Result<PagedResult<EnrollmentReportDto>>> GetEnrollmentReportAsync(EnrollmentReportQuery query);
    Task<Result<byte[]>> ExportEnrollmentReportCsvAsync(EnrollmentReportQuery query);

    Task<Result<PagedResult<CoursePerformanceReportDto>>> GetCoursePerformanceReportAsync(CoursePerformanceReportQuery query);
    Task<Result<byte[]>> ExportCoursePerformanceReportCsvAsync(CoursePerformanceReportQuery query);

    Task<Result<PagedResult<ELearningManagementSystem.Application.Features.AuditLogs.DTOs.AuditLogResponse>>> GetAuditActivityReportAsync(AuditActivityReportQuery query);
    Task<Result<byte[]>> ExportAuditActivityReportCsvAsync(AuditActivityReportQuery query);
    Task<Result<AuditActivityFilterOptionsDto>> GetAuditActivityFilterOptionsAsync();

    Task<Result<RevenueReportResponse>> GetRevenueReportAsync(RevenueReportQuery query);
    Task<Result<byte[]>> ExportRevenueReportCsvAsync(RevenueReportQuery query);

    Task<Result<ExamPerformanceReportResponse>> GetExamPerformanceReportAsync(ExamPerformanceReportQuery query);
    Task<Result<byte[]>> ExportExamPerformanceReportCsvAsync(ExamPerformanceReportQuery query);
}
