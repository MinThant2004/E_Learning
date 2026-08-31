using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Reports.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.Reports.Services;

public class ReportService : IReportService
{
    private readonly IAppDbContext _context;

    public ReportService(IAppDbContext context)
    {
        _context = context;
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // 1. Enrollment Report
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<Result<PagedResult<EnrollmentReportDto>>> GetEnrollmentReportAsync(EnrollmentReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<PagedResult<EnrollmentReportDto>>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        var dbQuery = _context.Enrollments
            .AsNoTracking()
            .Where(e => !e.Course.DeleteFlag && !e.User.DeleteFlag);

        // Date filter
        if (query.StartDate.HasValue)
        {
            var start = query.StartDate.Value.Date;
            dbQuery = dbQuery.Where(e => e.EnrollDate >= start);
        }
        if (query.EndDate.HasValue)
        {
            var end = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            dbQuery = dbQuery.Where(e => e.EnrollDate <= end);
        }

        // Search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(e =>
                e.User.FullName.ToLower().Contains(term) ||
                e.User.Email.ToLower().Contains(term) ||
                e.Course.Title.ToLower().Contains(term));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(query.StatusFilter) && !query.StatusFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var status = query.StatusFilter.ToLower();
            if (status == "completed")
            {
                dbQuery = dbQuery.Where(e => e.Completed);
            }
            else if (status == "in_progress")
            {
                dbQuery = dbQuery.Where(e => !e.Completed && e.LessonProgresses.Any(lp => lp.Completed));
            }
            else if (status == "not_started")
            {
                dbQuery = dbQuery.Where(e => !e.Completed && !e.LessonProgresses.Any(lp => lp.Completed));
            }
        }

        // Total Count
        var totalCount = await dbQuery.CountAsync();

        // Sorting
        dbQuery = (query.SortBy?.ToLower()) switch
        {
            "studentname" => query.SortDescending ? dbQuery.OrderByDescending(e => e.User.FullName) : dbQuery.OrderBy(e => e.User.FullName),
            "coursetitle" => query.SortDescending ? dbQuery.OrderByDescending(e => e.Course.Title) : dbQuery.OrderBy(e => e.Course.Title),
            "completeddate" => query.SortDescending ? dbQuery.OrderByDescending(e => e.CompletedDate) : dbQuery.OrderBy(e => e.CompletedDate),
            _ => query.SortDescending ? dbQuery.OrderByDescending(e => e.EnrollDate) : dbQuery.OrderBy(e => e.EnrollDate)
        };

        // Pagination
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EnrollmentReportDto
            {
                EnrollmentId = e.EnrollmentId,
                UserId = e.UserId,
                StudentName = e.User != null ? e.User.FullName : "Unknown",
                StudentEmail = e.User != null ? e.User.Email : "",
                CourseId = e.CourseId,
                CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
                EnrollDate = e.EnrollDate,
                Completed = e.Completed,
                CompletedDate = e.CompletedDate,
                CompletedLessonsCount = e.LessonProgresses.Count(lp => lp.Completed),
                TotalLessonsCount = e.Course != null ? e.Course.Lessons.Count(l => !l.DeleteFlag) : 0,
                CompletionStatus = e.Completed
                    ? "Completed"
                    : e.LessonProgresses.Any(lp => lp.Completed)
                        ? "In Progress"
                        : "Not Started"
            })
            .ToListAsync();

        // Calculate progress percentage
        foreach (var item in items)
        {
            if (item.Completed)
            {
                item.ProgressPercentage = 100.0;
            }
            else if (item.TotalLessonsCount > 0)
            {
                item.ProgressPercentage = Math.Round(((double)item.CompletedLessonsCount / item.TotalLessonsCount) * 100, 1);
            }
            else
            {
                item.ProgressPercentage = 0.0;
            }
        }

        var result = new PagedResult<EnrollmentReportDto>(items, totalCount, page, pageSize);
        return Result.Success(result);
    }

    public async Task<Result<byte[]>> ExportEnrollmentReportCsvAsync(EnrollmentReportQuery query)
    {
        // Fetch all matching items without pagination for export (capped at 5000 max for safety)
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetEnrollmentReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enrollment ID,Student Name,Student Email,Course Title,Enrollment Date,Status,Completion Date,Progress %");

        foreach (var row in reportResult.Value.Items)
        {
            sb.AppendLine($"{row.EnrollmentId},{EscapeCsv(row.StudentName)},{EscapeCsv(row.StudentEmail)},{EscapeCsv(row.CourseTitle)},{row.EnrollDate:dd-MM-yyyy hh:mm tt},{row.CompletionStatus},{(row.CompletedDate.HasValue ? row.CompletedDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "N/A")},{row.ProgressPercentage}%");
        }

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // 2. Course Performance Report
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<Result<PagedResult<CoursePerformanceReportDto>>> GetCoursePerformanceReportAsync(CoursePerformanceReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<PagedResult<CoursePerformanceReportDto>>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        var coursesQuery = _context.Courses
            .AsNoTracking()
            .Where(c => !c.DeleteFlag);

        // Search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            coursesQuery = coursesQuery.Where(c =>
                c.Title.ToLower().Contains(term) ||
                (c.Category != null && c.Category.CategoryName.ToLower().Contains(term)));
        }

        DateTime? start = query.StartDate?.Date;
        DateTime? end = query.EndDate?.Date.AddDays(1).AddTicks(-1);

        var projected = coursesQuery.Select(c => new
        {
            c.CourseId,
            c.Title,
            CategoryName = c.Category != null ? c.Category.CategoryName : "General",
            c.Status,
            TotalLessonsCount = c.Lessons.Count(l => !l.DeleteFlag),
            TotalQuizzesCount = c.Quizzes.Count(q => !q.DeleteFlag),
            FilteredEnrollments = c.Enrollments.Where(e =>
                (!start.HasValue || e.EnrollDate >= start.Value) &&
                (!end.HasValue || e.EnrollDate <= end.Value))
        });

        var rawList = await projected.ToListAsync();

        var dtoList = rawList.Select(c =>
        {
            var enrollments = c.FilteredEnrollments.ToList();
            var total = enrollments.Count;
            var completed = enrollments.Count(e => e.Completed);
            
            // In progress calculation
            var inProgress = enrollments.Count(e => !e.Completed && e.LessonProgresses.Any(lp => lp.Completed));
            var notStarted = Math.Max(0, total - completed - inProgress);

            var completionRate = total > 0 ? Math.Round(((double)completed / total) * 100, 1) : 0;

            return new CoursePerformanceReportDto
            {
                CourseId = c.CourseId,
                CourseTitle = c.Title,
                CategoryName = c.CategoryName,
                CourseStatus = c.Status,
                TotalEnrollments = total,
                CompletedCount = completed,
                InProgressCount = inProgress,
                NotStartedCount = notStarted,
                CompletionRatePercentage = completionRate,
                TotalLessonsCount = c.TotalLessonsCount,
                TotalQuizzesCount = c.TotalQuizzesCount
            };
        });

        // Sorting
        dtoList = (query.SortBy?.ToLower()) switch
        {
            "coursetitle" => query.SortDescending ? dtoList.OrderByDescending(c => c.CourseTitle) : dtoList.OrderBy(c => c.CourseTitle),
            "completionrate" => query.SortDescending ? dtoList.OrderByDescending(c => c.CompletionRatePercentage) : dtoList.OrderBy(c => c.CompletionRatePercentage),
            "completedcount" => query.SortDescending ? dtoList.OrderByDescending(c => c.CompletedCount) : dtoList.OrderBy(c => c.CompletedCount),
            _ => query.SortDescending ? dtoList.OrderByDescending(c => c.TotalEnrollments) : dtoList.OrderBy(c => c.TotalEnrollments)
        };

        var totalCount = dtoList.Count();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var pagedItems = dtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new PagedResult<CoursePerformanceReportDto>(pagedItems, totalCount, page, pageSize);

        return Result.Success(result);
    }

    public async Task<Result<byte[]>> ExportCoursePerformanceReportCsvAsync(CoursePerformanceReportQuery query)
    {
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetCoursePerformanceReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Course ID,Course Title,Category,Status,Total Enrollments,Completed,In Progress,Not Started,Completion Rate %,Total Lessons,Total Quizzes");

        foreach (var row in reportResult.Value.Items)
        {
            var statusStr = row.CourseStatus ? "Active" : "Inactive";
            sb.AppendLine($"{row.CourseId},{EscapeCsv(row.CourseTitle)},{EscapeCsv(row.CategoryName)},{statusStr},{row.TotalEnrollments},{row.CompletedCount},{row.InProgressCount},{row.NotStartedCount},{row.CompletionRatePercentage}%,{row.TotalLessonsCount},{row.TotalQuizzesCount}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // 3. Audit Activity Report
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<Result<PagedResult<ELearningManagementSystem.Application.Features.AuditLogs.DTOs.AuditLogResponse>>> GetAuditActivityReportAsync(AuditActivityReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<PagedResult<ELearningManagementSystem.Application.Features.AuditLogs.DTOs.AuditLogResponse>>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        var dbQuery = _context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking();

        // Apply filters
        if (query.StartDate.HasValue)
        {
            var start = query.StartDate.Value.Date;
            dbQuery = dbQuery.Where(a => a.CreatedAt >= start);
        }
        if (query.EndDate.HasValue)
        {
            var end = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            dbQuery = dbQuery.Where(a => a.CreatedAt <= end);
        }

        if (query.UserIdFilter.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.UserId == query.UserIdFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ActionFilter))
        {
            dbQuery = dbQuery.Where(a => a.Action == query.ActionFilter.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.TableNameFilter))
        {
            dbQuery = dbQuery.Where(a => a.TableName == query.TableNameFilter.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(a =>
                a.Action.ToLower().Contains(search) ||
                a.TableName.ToLower().Contains(search) ||
                a.User.Email.ToLower().Contains(search) ||
                a.User.FullName.ToLower().Contains(search)
            );
        }

        // Total Count
        var totalCount = await dbQuery.CountAsync();

        // Sorting
        dbQuery = (query.SortBy?.ToLower()) switch
        {
            "user" => query.SortDescending
                ? dbQuery.OrderByDescending(a => a.User.FullName)
                : dbQuery.OrderBy(a => a.User.FullName),
            "action" => query.SortDescending
                ? dbQuery.OrderByDescending(a => a.Action)
                : dbQuery.OrderBy(a => a.Action),
            "target" => query.SortDescending
                ? dbQuery.OrderByDescending(a => a.TableName)
                : dbQuery.OrderBy(a => a.TableName),
            _ => query.SortDescending
                ? dbQuery.OrderByDescending(a => a.CreatedAt)
                : dbQuery.OrderBy(a => a.CreatedAt)
        };

        // Pagination
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ELearningManagementSystem.Application.Features.AuditLogs.DTOs.AuditLogResponse
            {
                AuditLogId = a.AuditLogId,
                UserId = a.UserId,
                UserEmail = a.User != null ? a.User.Email : string.Empty,
                UserFullName = a.User != null ? a.User.FullName : "System",
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<ELearningManagementSystem.Application.Features.AuditLogs.DTOs.AuditLogResponse>(items, totalCount, page, pageSize);
        return Result.Success(result);
    }

    public async Task<Result<byte[]>> ExportAuditActivityReportCsvAsync(AuditActivityReportQuery query)
    {
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetAuditActivityReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Log ID,User Name,User Email,Action,Target Table,Record ID,Date Time");

        foreach (var row in reportResult.Value.Items)
        {
            sb.AppendLine($"{row.AuditLogId},{EscapeCsv(row.UserFullName)},{EscapeCsv(row.UserEmail)},{row.Action},{row.TableName},{row.RecordId},{row.CreatedAt:dd-MM-yyyy hh:mm:ss tt}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<Result<AuditActivityFilterOptionsDto>> GetAuditActivityFilterOptionsAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.DeleteFlag)
            .Select(u => new ReportUserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email
            })
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var actions = await _context.AuditLogs
            .AsNoTracking()
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();

        var tables = await _context.AuditLogs
            .AsNoTracking()
            .Select(a => a.TableName)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        var options = new AuditActivityFilterOptionsDto
        {
            Users = users,
            Actions = actions,
            TableNames = tables
        };

        return Result.Success(options);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // 4. Revenue (Exam Payment) Report
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<Result<RevenueReportResponse>> GetRevenueReportAsync(RevenueReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<RevenueReportResponse>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        DateTime? start = query.StartDate?.Date;
        DateTime? end = query.EndDate?.Date.AddDays(1).AddTicks(-1);

        var examsQuery = _context.CourseExams
            .AsNoTracking()
            .Where(e => !e.DeleteFlag && !e.Course.DeleteFlag);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            examsQuery = examsQuery.Where(e =>
                e.Title.ToLower().Contains(term) ||
                e.Course.Title.ToLower().Contains(term));
        }

        var projected = examsQuery.Select(e => new
        {
            e.ExamId,
            e.Title,
            e.CourseId,
            e.ExamFee,
            CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
            FilteredPayments = e.ExamPayments.Where(p =>
                !p.DeleteFlag &&
                !p.User.DeleteFlag &&
                (!start.HasValue || p.CreatedAt >= start.Value) &&
                (!end.HasValue || p.CreatedAt <= end.Value))
        });

        var rawList = await projected.ToListAsync();

        var dtoList = rawList.Select(e =>
        {
            var payments = e.FilteredPayments.ToList();
            var totalPayments = payments.Count;
            var approved = payments.Where(p => p.Status == "Approved").ToList();
            var pending = payments.Where(p => p.Status == "Pending").ToList();
            var rejected = payments.Where(p => p.Status == "Rejected").ToList();

            return new RevenueReportDto
            {
                ExamId = e.ExamId,
                ExamTitle = e.Title,
                CourseId = e.CourseId,
                CourseTitle = e.CourseTitle,
                ExamFee = e.ExamFee,
                TotalPayments = totalPayments,
                ApprovedCount = approved.Count,
                PendingCount = pending.Count,
                RejectedCount = rejected.Count,
                TotalAmount = payments.Sum(p => (decimal)p.Amount),
                ApprovedRevenue = approved.Sum(p => (decimal)p.Amount),
                PendingAmount = pending.Sum(p => (decimal)p.Amount),
                RejectedAmount = rejected.Sum(p => (decimal)p.Amount)
            };
        });

        // Sorting
        dtoList = (query.SortBy?.ToLower()) switch
        {
            "examtitle" => query.SortDescending ? dtoList.OrderByDescending(d => d.ExamTitle) : dtoList.OrderBy(d => d.ExamTitle),
            "coursetitle" => query.SortDescending ? dtoList.OrderByDescending(d => d.CourseTitle) : dtoList.OrderBy(d => d.CourseTitle),
            "approvedrevenue" => query.SortDescending ? dtoList.OrderByDescending(d => d.ApprovedRevenue) : dtoList.OrderBy(d => d.ApprovedRevenue),
            "pendingamount" => query.SortDescending ? dtoList.OrderByDescending(d => d.PendingAmount) : dtoList.OrderBy(d => d.PendingAmount),
            "approvedcount" => query.SortDescending ? dtoList.OrderByDescending(d => d.ApprovedCount) : dtoList.OrderBy(d => d.ApprovedCount),
            _ => query.SortDescending ? dtoList.OrderByDescending(d => d.TotalAmount) : dtoList.OrderBy(d => d.TotalAmount)
        };

        var totalCount = dtoList.Count();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pagedItems = dtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var summary = new RevenueReportSummaryDto
        {
            TotalApprovedRevenue = dtoList.Sum(d => d.ApprovedRevenue),
            TotalPendingAmount = dtoList.Sum(d => d.PendingAmount),
            TotalAmount = dtoList.Sum(d => d.TotalAmount),
            TotalApprovedCount = dtoList.Sum(d => d.ApprovedCount),
            TotalPendingCount = dtoList.Sum(d => d.PendingCount)
        };

        var response = new RevenueReportResponse
        {
            Data = new PagedResult<RevenueReportDto>(pagedItems, totalCount, page, pageSize),
            Summary = summary
        };

        return Result.Success(response);
    }

    public async Task<Result<byte[]>> ExportRevenueReportCsvAsync(RevenueReportQuery query)
    {
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetRevenueReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Exam Title,Course Title,Exam Fee,Total Payments,Approved,Pending,Rejected,Total Amount,Approved Revenue,Pending Amount,Rejected Amount");

        foreach (var row in reportResult.Value.Data.Items)
        {
            sb.AppendLine($"{EscapeCsv(row.ExamTitle)},{EscapeCsv(row.CourseTitle)},{row.ExamFee:N2},{row.TotalPayments},{row.ApprovedCount},{row.PendingCount},{row.RejectedCount},{row.TotalAmount:N2},{row.ApprovedRevenue:N2},{row.PendingAmount:N2},{row.RejectedAmount:N2}");
        }

        var summary = reportResult.Value.Summary;
        sb.AppendLine($"TOTAL,,,,,{summary.TotalApprovedCount},{summary.TotalPendingCount},,{summary.TotalApprovedRevenue:N2},{summary.TotalPendingAmount:N2},");

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 5. Exam Performance Report
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<Result<ExamPerformanceReportResponse>> GetExamPerformanceReportAsync(ExamPerformanceReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<ExamPerformanceReportResponse>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        DateTime? start = query.StartDate?.Date;
        DateTime? end = query.EndDate?.Date.AddDays(1).AddTicks(-1);

        var examsQuery = _context.CourseExams
            .AsNoTracking()
            .Where(e => !e.DeleteFlag && !e.Course.DeleteFlag);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            examsQuery = examsQuery.Where(e =>
                e.Title.ToLower().Contains(term) ||
                e.Course.Title.ToLower().Contains(term));
        }

        var projected = examsQuery.Select(e => new
        {
            e.ExamId,
            e.Title,
            e.CourseId,
            e.PassingScore,
            e.MaxAttempts,
            e.ExamFee,
            CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
            FilteredAttempts = e.ExamAttempts.Where(a =>
                !a.DeleteFlag &&
                a.SubmittedAt.HasValue &&
                (!start.HasValue || a.SubmittedAt.Value >= start.Value) &&
                (!end.HasValue || a.SubmittedAt.Value <= end.Value))
        });

        var rawList = await projected.ToListAsync();

        var dtoList = rawList.Select(e =>
        {
            var attempts = e.FilteredAttempts.ToList();
            var total = attempts.Count;
            var passed = attempts.Count(a => a.Passed == true);
            var failed = attempts.Count(a => a.Passed == false);

            var avgScore = total > 0 ? Math.Round(attempts.Where(a => a.Score.HasValue).Average(a => (double)a.Score!.Value), 1) : 0;
            var highest = total > 0 ? Math.Round(attempts.Where(a => a.Score.HasValue).Max(a => (double)a.Score!.Value), 1) : 0;
            var passRate = total > 0 ? Math.Round(((double)passed / total) * 100, 1) : 0;
            var failRate = total > 0 ? Math.Round(((double)failed / total) * 100, 1) : 0;

            return new ExamPerformanceReportDto
            {
                ExamId = e.ExamId,
                ExamTitle = e.Title,
                CourseId = e.CourseId,
                CourseTitle = e.CourseTitle,
                PassingScore = e.PassingScore,
                MaxAttempts = e.MaxAttempts,
                ExamFee = e.ExamFee,
                TotalAttempts = total,
                PassedCount = passed,
                FailedCount = failed,
                AverageScore = total > 0 ? avgScore : 0,
                PassRatePercentage = passRate,
                FailRatePercentage = failRate,
                HighestScore = highest
            };
        });

        // Sorting
        dtoList = (query.SortBy?.ToLower()) switch
        {
            "examtitle" => query.SortDescending ? dtoList.OrderByDescending(d => d.ExamTitle) : dtoList.OrderBy(d => d.ExamTitle),
            "coursetitle" => query.SortDescending ? dtoList.OrderByDescending(d => d.CourseTitle) : dtoList.OrderBy(d => d.CourseTitle),
            "passrate" => query.SortDescending ? dtoList.OrderByDescending(d => d.PassRatePercentage) : dtoList.OrderBy(d => d.PassRatePercentage),
            "averagescore" => query.SortDescending ? dtoList.OrderByDescending(d => d.AverageScore) : dtoList.OrderBy(d => d.AverageScore),
            "passedcount" => query.SortDescending ? dtoList.OrderByDescending(d => d.PassedCount) : dtoList.OrderBy(d => d.PassedCount),
            _ => query.SortDescending ? dtoList.OrderByDescending(d => d.TotalAttempts) : dtoList.OrderBy(d => d.TotalAttempts)
        };

        var totalCount = dtoList.Count();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pagedItems = dtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var summary = new ExamPerformanceReportSummaryDto
        {
            TotalAttempts = dtoList.Sum(d => d.TotalAttempts),
            PassedCount = dtoList.Sum(d => d.PassedCount),
            FailedCount = dtoList.Sum(d => d.FailedCount),
            AverageScore = dtoList.Sum(d => d.TotalAttempts) > 0
                ? Math.Round(dtoList.Sum(d => d.AverageScore * d.TotalAttempts) / dtoList.Sum(d => d.TotalAttempts), 1)
                : 0,
            PassRatePercentage = dtoList.Sum(d => d.TotalAttempts) > 0
                ? Math.Round(((double)dtoList.Sum(d => d.PassedCount) / dtoList.Sum(d => d.TotalAttempts)) * 100, 1)
                : 0
        };

        var response = new ExamPerformanceReportResponse
        {
            Data = new PagedResult<ExamPerformanceReportDto>(pagedItems, totalCount, page, pageSize),
            Summary = summary
        };

        return Result.Success(response);
    }

    public async Task<Result<byte[]>> ExportExamPerformanceReportCsvAsync(ExamPerformanceReportQuery query)
    {
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetExamPerformanceReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Exam Title,Course Title,Passing Score,Max Attempts,Exam Fee,Total Attempts,Passed,Failed,Average Score %,Pass Rate %,Fail Rate %,Highest Score %");

        foreach (var row in reportResult.Value.Data.Items)
        {
            sb.AppendLine($"{EscapeCsv(row.ExamTitle)},{EscapeCsv(row.CourseTitle)},{row.PassingScore},{row.MaxAttempts},{row.ExamFee:N2},{row.TotalAttempts},{row.PassedCount},{row.FailedCount},{row.AverageScore}%,{row.PassRatePercentage}%,{row.FailRatePercentage}%,{row.HighestScore}%");
        }

        var summary = reportResult.Value.Summary;
        sb.AppendLine($"TOTAL,,,,,{summary.TotalAttempts},{summary.PassedCount},{summary.FailedCount},{summary.AverageScore}%,{summary.PassRatePercentage}%,,");

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n") || text.Contains("\r"))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return text;
    }
}

