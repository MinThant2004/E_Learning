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

    // ─────────────────────────────────────────────────────────────────────────────
    // 1. Enrollment Report
    // ─────────────────────────────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────────────────────────────
    // 2. Course Performance Report
    // ─────────────────────────────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────────────────────────────
    // 3. Quiz Performance Report
    // ─────────────────────────────────────────────────────────────────────────────
    public async Task<Result<PagedResult<QuizPerformanceReportDto>>> GetQuizPerformanceReportAsync(QuizPerformanceReportQuery query)
    {
        if (query.StartDate.HasValue && query.EndDate.HasValue && query.EndDate.Value.Date < query.StartDate.Value.Date)
        {
            return Result.Failure<PagedResult<QuizPerformanceReportDto>>("ValidationError: End Date cannot be earlier than Start Date.");
        }

        var quizzesQuery = _context.Quizzes
            .AsNoTracking()
            .Where(q => !q.DeleteFlag && !q.Course.DeleteFlag);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            quizzesQuery = quizzesQuery.Where(q =>
                q.Title.ToLower().Contains(term) ||
                q.Course.Title.ToLower().Contains(term));
        }

        DateTime? start = query.StartDate?.Date;
        DateTime? end = query.EndDate?.Date.AddDays(1).AddTicks(-1);

        var projected = quizzesQuery.Select(q => new
        {
            q.QuizId,
            q.Title,
            CourseId = q.CourseId,
            CourseTitle = q.Course != null ? q.Course.Title : "Unknown",
            q.PassingScore,
            FilteredAttempts = q.QuizAttempts.Where(a =>
                (!start.HasValue || a.SubmittedAt >= start.Value) &&
                (!end.HasValue || a.SubmittedAt <= end.Value))
        });

        var rawList = await projected.ToListAsync();

        var dtoList = rawList.Select(q =>
        {
            var attempts = q.FilteredAttempts.ToList();
            var totalAttempts = attempts.Count;
            var passedCount = attempts.Count(a => a.Passed);
            var failedCount = totalAttempts - passedCount;

            var avgScore = totalAttempts > 0 ? Math.Round(attempts.Average(a => (double)a.Score), 1) : 0;
            var passRate = totalAttempts > 0 ? Math.Round(((double)passedCount / totalAttempts) * 100, 1) : 0;
            var failRate = totalAttempts > 0 ? Math.Round(((double)failedCount / totalAttempts) * 100, 1) : 0;
            var highestScore = totalAttempts > 0 ? Math.Round(attempts.Max(a => (double)a.Score), 1) : 0;

            return new QuizPerformanceReportDto
            {
                QuizId = q.QuizId,
                QuizTitle = q.Title,
                CourseId = q.CourseId,
                CourseTitle = q.CourseTitle,
                PassingScore = q.PassingScore,
                TotalAttempts = totalAttempts,
                PassedAttempts = passedCount,
                FailedAttempts = failedCount,
                AverageScore = avgScore,
                PassRatePercentage = passRate,
                FailRatePercentage = failRate,
                HighestScore = highestScore
            };
        });

        // Sorting
        dtoList = (query.SortBy?.ToLower()) switch
        {
            "quiztitle" => query.SortDescending ? dtoList.OrderByDescending(q => q.QuizTitle) : dtoList.OrderBy(q => q.QuizTitle),
            "coursetitle" => query.SortDescending ? dtoList.OrderByDescending(q => q.CourseTitle) : dtoList.OrderBy(q => q.CourseTitle),
            "averagescore" => query.SortDescending ? dtoList.OrderByDescending(q => q.AverageScore) : dtoList.OrderBy(q => q.AverageScore),
            "passrate" => query.SortDescending ? dtoList.OrderByDescending(q => q.PassRatePercentage) : dtoList.OrderBy(q => q.PassRatePercentage),
            _ => query.SortDescending ? dtoList.OrderByDescending(q => q.TotalAttempts) : dtoList.OrderBy(q => q.TotalAttempts)
        };

        var totalCount = dtoList.Count();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var pagedItems = dtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new PagedResult<QuizPerformanceReportDto>(pagedItems, totalCount, page, pageSize);

        return Result.Success(result);
    }

    public async Task<Result<byte[]>> ExportQuizPerformanceReportCsvAsync(QuizPerformanceReportQuery query)
    {
        query.Page = 1;
        query.PageSize = 5000;

        var reportResult = await GetQuizPerformanceReportAsync(query);
        if (reportResult.IsFailure || reportResult.Value == null)
        {
            return Result.Failure<byte[]>(reportResult.Error ?? "Failed to generate report.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Quiz ID,Quiz Title,Course Title,Passing Score %,Total Attempts,Passed,Failed,Average Score %,Pass Rate %,Highest Score %");

        foreach (var row in reportResult.Value.Items)
        {
            sb.AppendLine($"{row.QuizId},{EscapeCsv(row.QuizTitle)},{EscapeCsv(row.CourseTitle)},{row.PassingScore}%,{row.TotalAttempts},{row.PassedAttempts},{row.FailedAttempts},{row.AverageScore}%,{row.PassRatePercentage}%,{row.HighestScore}%");
        }

        return Result.Success(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 4. Audit Activity Report
    // ─────────────────────────────────────────────────────────────────────────────
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
