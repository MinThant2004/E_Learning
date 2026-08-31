using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.AdminDashboard.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.AdminDashboard.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermissionService _permissionService;

        public AdminDashboardService(
            IAppDbContext context,
            ICurrentUserService currentUserService,
            IPermissionService permissionService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _permissionService = permissionService;
        }

        public async Task<Result<AdminDashboardResponse>> GetDashboardAsync(string range = "monthly", DateTime? customStartDate = null, DateTime? customEndDate = null)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Result.Failure<AdminDashboardResponse>("AccountNotFound: User identity not found.");
            }

            var permissions = await _permissionService.GetPermissionsForUserAsync(userId.Value);
            
            // Allow access if user holds any administrative read or manage permission
            if (!permissions.Any(p => p == "User.Read" || p == "Role.Read" || p == "Category.Read" || p == "Course.Read" || p == "Lesson.Read" || p == "Quiz.Read" || p == "AuditLog.Read" || p == "Course.Update" || p == "Lesson.Update" || p == "Quiz.Update"))
            {
                return Result.Failure<AdminDashboardResponse>("PermissionDenied: You do not have access to the admin dashboard.");
            }

            // ── Unified Date Range Parsing ──────────────────────────────────────────
            // Presets match the Reports pages: daily = today, weekly = last 7 days,
            // monthly = last 30 days, custom = explicit start/end.
            var now = DateTime.UtcNow;
            var normalizedRange = (range ?? "monthly").ToLowerInvariant();
            DateTime filterStartDate;
            DateTime filterEndDate = now;

            if (normalizedRange == "custom")
            {
                if (!customStartDate.HasValue || !customEndDate.HasValue)
                {
                    return Result.Failure<AdminDashboardResponse>("ValidationError: Both Start Date and End Date are required for custom range.");
                }
                if (customEndDate.Value.Date < customStartDate.Value.Date)
                {
                    return Result.Failure<AdminDashboardResponse>("ValidationError: End Date cannot be earlier than Start Date.");
                }
                filterStartDate = customStartDate.Value.Date;
                filterEndDate = customEndDate.Value.Date.AddDays(1).AddTicks(-1); // end of day
            }
            else if (normalizedRange == "daily")
            {
                filterStartDate = now.Date;
            }
            else if (normalizedRange == "weekly")
            {
                filterStartDate = now.Date.AddDays(-6);
            }
            else // Default "monthly"
            {
                normalizedRange = "monthly";
                filterStartDate = now.Date.AddDays(-29);
            }

            var response = new AdminDashboardResponse();

            // Find first active course to provide direct deep-linking for Lessons and Quizzes KPI cards
            var firstActiveCourseId = await _context.Courses
                .AsNoTracking()
                .Where(c => !c.DeleteFlag)
                .Select(c => c.CourseId)
                .FirstOrDefaultAsync();

            // 1. Categories Metric & Quick Action
            if (permissions.Contains("Category.Read"))
            {
                var activeCategories = await _context.Categories.AsNoTracking().CountAsync(c => !c.DeleteFlag);
                var archivedCategories = await _context.Categories.AsNoTracking().CountAsync(c => c.DeleteFlag);

                response.Metrics.Add("Categories", new ContentMetricResponse
                {
                    Title = "Categories",
                    Icon = "folder",
                    ActiveCount = activeCategories,
                    ArchivedCount = archivedCategories,
                    Route = "/admin/categories"
                });

                if (permissions.Contains("Category.Create"))
                {
                    response.QuickActions.Add(new AdminQuickActionResponse
                    {
                        Title = "New Category",
                        Description = "Create a new course category.",
                        Icon = "plus",
                        Route = "/admin/categories"
                    });
                }
            }

            // 2. Courses Metric & Quick Action
            if (permissions.Contains("Course.Read"))
            {
                var activeCourses = await _context.Courses.AsNoTracking().CountAsync(c => !c.DeleteFlag);
                var archivedCourses = await _context.Courses.AsNoTracking().CountAsync(c => c.DeleteFlag);

                response.Metrics.Add("Courses", new ContentMetricResponse
                {
                    Title = "Courses",
                    Icon = "book",
                    ActiveCount = activeCourses,
                    ArchivedCount = archivedCourses,
                    Route = "/admin/courses"
                });

                if (permissions.Contains("Course.Create"))
                {
                    response.QuickActions.Add(new AdminQuickActionResponse
                    {
                        Title = "New Course",
                        Description = "Create a new learning course.",
                        Icon = "plus",
                        Route = "/admin/courses"
                    });
                }
            }

            // 3. Lessons Metric
            if (permissions.Contains("Lesson.Read"))
            {
                var activeLessons = await _context.Lessons.AsNoTracking().CountAsync(l => !l.DeleteFlag);
                var archivedLessons = await _context.Lessons.AsNoTracking().CountAsync(l => l.DeleteFlag);

                response.Metrics.Add("Lessons", new ContentMetricResponse
                {
                    Title = "Lessons",
                    Icon = "document-text",
                    ActiveCount = activeLessons,
                    ArchivedCount = archivedLessons,
                    Route = firstActiveCourseId > 0 ? $"/admin/courses/{firstActiveCourseId}/lessons" : "/admin/courses"
                });
            }

            // 4. Quizzes Metric
            if (permissions.Contains("Quiz.Read"))
            {
                var activeQuizzes = await _context.Quizzes.AsNoTracking().CountAsync(q => !q.DeleteFlag);
                var archivedQuizzes = await _context.Quizzes.AsNoTracking().CountAsync(q => q.DeleteFlag);

                response.Metrics.Add("Quizzes", new ContentMetricResponse
                {
                    Title = "Quizzes",
                    Icon = "check-circle",
                    ActiveCount = activeQuizzes,
                    ArchivedCount = archivedQuizzes,
                    Route = firstActiveCourseId > 0 ? $"/admin/courses/{firstActiveCourseId}/quizzes" : "/admin/courses"
                });
            }

            // 5. Users Metric (Real database soft-delete count) & Quick Action
            if (permissions.Contains("User.Read"))
            {
                var activeUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.DeleteFlag);
                var archivedUsers = await _context.Users.AsNoTracking().CountAsync(u => u.DeleteFlag);

                response.Metrics.Add("Users", new ContentMetricResponse
                {
                    Title = "Users",
                    Icon = "users",
                    ActiveCount = activeUsers,
                    ArchivedCount = archivedUsers,
                    Route = "/admin/users"
                });

                if (permissions.Contains("User.Update") || permissions.Contains("Permission.Assign"))
                {
                    response.QuickActions.Add(new AdminQuickActionResponse
                    {
                        Title = "Manage Users",
                        Description = "Review and edit user access & roles.",
                        Icon = "users",
                        Route = "/admin/users"
                    });
                }
            }

            // 6. Roles Metric & Quick Action
            if (permissions.Contains("Role.Read"))
            {
                var activeRoles = await _context.Roles.AsNoTracking().CountAsync();
                response.Metrics.Add("Roles", new ContentMetricResponse
                {
                    Title = "Roles",
                    Icon = "shield-check",
                    ActiveCount = activeRoles,
                    ArchivedCount = 0,
                    Route = "/admin/roles"
                });

                if (permissions.Contains("Role.Create") || permissions.Contains("Permission.Assign"))
                {
                    response.QuickActions.Add(new AdminQuickActionResponse
                    {
                        Title = "Manage Roles",
                        Description = "Configure system roles and permissions.",
                        Icon = "shield-check",
                        Route = "/admin/roles"
                    });
                }
            }

            // 7. Audit Logs Metric & Quick Action
            if (permissions.Contains("AuditLog.Read"))
            {
                var totalLogs = await _context.AuditLogs.AsNoTracking().CountAsync();
                response.Metrics.Add("AuditLogs", new ContentMetricResponse
                {
                    Title = "Audit Logs",
                    Icon = "clipboard-list",
                    ActiveCount = totalLogs,
                    ArchivedCount = 0,
                    Route = "/admin/audit-logs"
                });

                response.QuickActions.Add(new AdminQuickActionResponse
                {
                    Title = "View Audit Logs",
                    Icon = "clipboard-list",
                    Description = "Inspect system activity logs & security events.",
                    Route = "/admin/audit-logs"
                });
            }

            // ── ANALYTICS 1: Enrollment Trend (uses unified filterStartDate/filterEndDate) ──
            if (permissions.Contains("Course.Read"))
            {
                var enrollments = await _context.Enrollments
                    .AsNoTracking()
                    .Where(e => e.EnrollDate >= filterStartDate && e.EnrollDate <= filterEndDate)
                    .ToListAsync();

                var trendItems = new List<EnrollmentTrendItemDto>();
                var totalDays = (filterEndDate.Date - filterStartDate.Date).Days + 1;

                // Group by month only for long custom ranges (> 60 days)
                if (normalizedRange == "custom" && totalDays > 60)
                {
                    var monthStart = new DateTime(filterStartDate.Year, filterStartDate.Month, 1);
                    var monthEnd = new DateTime(filterEndDate.Year, filterEndDate.Month, 1);
                    for (var m = monthStart; m <= monthEnd; m = m.AddMonths(1))
                    {
                        var count = enrollments.Count(e => e.EnrollDate.Year == m.Year && e.EnrollDate.Month == m.Month);
                        trendItems.Add(new EnrollmentTrendItemDto
                        {
                            Date = m,
                            DateLabel = m.ToString("dd-MM-yyyy"),
                            Count = count
                        });
                    }
                }
                else
                {
                    // Group by day
                    for (int i = 0; i < totalDays; i++)
                    {
                        var dayDate = filterStartDate.AddDays(i);
                        if (dayDate.Date > filterEndDate.Date) break;
                        var count = enrollments.Count(e => e.EnrollDate.Date == dayDate.Date);
                        trendItems.Add(new EnrollmentTrendItemDto
                        {
                            Date = dayDate,
                            DateLabel = dayDate.ToString("dd-MM-yyyy"),
                            Count = count
                        });
                    }
                }

                response.EnrollmentAnalytics = new EnrollmentAnalyticsResponse
                {
                    Range = normalizedRange,
                    Trends = trendItems
                };

                // ── ANALYTICS 2: Course Completion Breakdown (date-filtered) ─────────
                var periodEnrollmentIds = enrollments.Select(e => e.EnrollmentId).ToList();
                var totalEnrollments = enrollments.Count;
                var completedCount = enrollments.Count(e => e.Completed);

                var inProgressCount = 0;
                if (periodEnrollmentIds.Any())
                {
                    var inProgressEnrollmentIds = await _context.LessonProgresses
                        .AsNoTracking()
                        .Where(lp => lp.Completed && periodEnrollmentIds.Contains(lp.EnrollmentId))
                        .Select(lp => lp.EnrollmentId)
                        .Distinct()
                        .ToListAsync();

                    inProgressCount = enrollments.Count(e => !e.Completed && inProgressEnrollmentIds.Contains(e.EnrollmentId));
                }

                var notCompletedCount = Math.Max(0, totalEnrollments - completedCount - inProgressCount);
                var completionRate = totalEnrollments > 0
                    ? Math.Round(((double)completedCount / totalEnrollments) * 100, 1)
                    : 0;

                response.CompletionStats = new CourseCompletionStatsDto
                {
                    TotalEnrollments = totalEnrollments,
                    CompletedCount = completedCount,
                    InProgressCount = inProgressCount,
                    NotCompletedCount = notCompletedCount,
                    CompletionRatePercentage = completionRate
                };

                // ── PHASE 3 WIDGET 1: Popular Courses Table (Top 5, date-filtered) ──
                var popularCoursesData = await _context.Courses
                    .AsNoTracking()
                    .Where(c => !c.DeleteFlag)
                    .Select(c => new
                    {
                        c.CourseId,
                        c.Title,
                        CategoryName = c.Category != null ? c.Category.CategoryName : "General",
                        TotalStudents = c.Enrollments.Count(e => e.EnrollDate >= filterStartDate && e.EnrollDate <= filterEndDate),
                        CompletedStudents = c.Enrollments.Count(e => e.EnrollDate >= filterStartDate && e.EnrollDate <= filterEndDate && e.Completed)
                    })
                    .OrderByDescending(c => c.TotalStudents)
                    .Take(5)
                    .ToListAsync();

                response.PopularCourses = popularCoursesData.Select(c => new PopularCourseSummaryDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    CategoryName = c.CategoryName,
                    EnrolledStudentsCount = c.TotalStudents,
                    CompletionRatePercentage = c.TotalStudents > 0
                        ? Math.Round(((double)c.CompletedStudents / c.TotalStudents) * 100, 1)
                        : 0
                }).ToList();
            }

            // ── ANALYTICS 3: Quiz Performance Efficacy (date-filtered) ──────────────
            if (permissions.Contains("Quiz.Read"))
            {
                var attempts = await _context.QuizAttempts
                    .AsNoTracking()
                    .Where(a => a.SubmittedAt >= filterStartDate && a.SubmittedAt <= filterEndDate)
                    .ToListAsync();

                var totalAttempts = attempts.Count;
                var passedCount = attempts.Count(a => a.Passed);
                var failedCount = totalAttempts - passedCount;

                double avgScore = totalAttempts > 0
                    ? Math.Round(attempts.Average(a => (double)a.Score), 1)
                    : 0;

                double passRate = totalAttempts > 0
                    ? Math.Round(((double)passedCount / totalAttempts) * 100, 1)
                    : 0;

                double failRate = totalAttempts > 0
                    ? Math.Round(((double)failedCount / totalAttempts) * 100, 1)
                    : 0;

                double highestScore = totalAttempts > 0
                    ? Math.Round(attempts.Max(a => (double)a.Score), 1)
                    : 0;

                var totalQuizzes = await _context.Quizzes.AsNoTracking().CountAsync(q => !q.DeleteFlag);

                response.QuizPerformance = new QuizPerformanceStatsDto
                {
                    TotalAttempts = totalAttempts,
                    PassedCount = passedCount,
                    FailedCount = failedCount,
                    AverageScore = avgScore,
                    PassRatePercentage = passRate,
                    FailRatePercentage = failRate,
                    HighestScore = highestScore,
                    TotalQuizzes = totalQuizzes,
                    StatusHealth = passRate >= 70 ? "Healthy" : passRate >= 40 ? "Moderate" : "Needs Review"
                };
            }

            // ── PHASE 3 WIDGET 2: Humanized Recent Activity Stream (Top 8, date-filtered) ──
            if (permissions.Contains("AuditLog.Read"))
            {
                var rawLogs = await _context.AuditLogs
                    .AsNoTracking()
                    .Where(a => a.CreatedAt >= filterStartDate && a.CreatedAt <= filterEndDate)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(8)
                    .Select(a => new
                    {
                        a.AuditLogId,
                        a.UserId,
                        UserFullName = a.User != null ? a.User.FullName : "System",
                        a.Action,
                        a.TableName,
                        a.RecordId,
                        a.CreatedAt
                    })
                    .ToListAsync();

                response.RecentActivities = rawLogs.Select(log =>
                {
                    var (actionText, iconType) = HumanizeAuditLog(log.UserFullName, log.Action, log.TableName, log.RecordId);
                    return new RecentActivityItemDto
                    {
                        AuditLogId = log.AuditLogId,
                        UserFullName = log.UserFullName,
                        ActionText = actionText,
                        RelativeTime = GetRelativeTime(log.CreatedAt),
                        IconType = iconType,
                        CreatedAt = log.CreatedAt
                    };
                }).ToList();
            }

            return Result.Success(response);
        }

        private static (string ActionText, string IconType) HumanizeAuditLog(string user, string action, string table, int recordId)
        {
            var act = (action ?? "").ToUpperInvariant();
            var tbl = (table ?? "").Trim();
            
            var isUserTable = tbl.Equals("User", StringComparison.OrdinalIgnoreCase) || tbl.Equals("Users", StringComparison.OrdinalIgnoreCase);
            var isCourseTable = tbl.Equals("Course", StringComparison.OrdinalIgnoreCase) || tbl.Equals("Courses", StringComparison.OrdinalIgnoreCase);
            var isCategoryTable = tbl.Equals("Category", StringComparison.OrdinalIgnoreCase) || tbl.Equals("Categories", StringComparison.OrdinalIgnoreCase);
            var isQuizTable = tbl.Equals("Quiz", StringComparison.OrdinalIgnoreCase) || tbl.Equals("Quizzes", StringComparison.OrdinalIgnoreCase);

            if (act.Contains("CREATE") || act.Contains("INSERT") || act.Contains("REGISTER"))
            {
                if (isUserTable)
                    return ($"User account '{user}' was registered", "create");
                if (isCourseTable)
                    return ($"{user} created a new course", "create");
                if (isCategoryTable)
                    return ($"{user} created a new category", "create");
                if (isQuizTable)
                    return ($"{user} published a new quiz", "create");
                
                var cleanTableName = tbl.TrimEnd('s', 'S').ToLowerInvariant();
                return ($"{user} created a new {cleanTableName}", "create");
            }

            if (act.Contains("DELETE") || act.Contains("ARCHIVE") || act.Contains("SOFT_DELETE"))
            {
                var cleanTableName = tbl.TrimEnd('s', 'S').ToLowerInvariant();
                return ($"{user} archived {cleanTableName} #{recordId}", "delete");
            }

            if (act.Contains("ROLE") || act.Contains("PERMISSION") || act.Contains("ASSIGN"))
            {
                return ($"{user} updated role permissions for #{recordId}", "role");
            }

            if (act.Contains("UPDATE") || act.Contains("EDIT"))
            {
                var cleanTableName = tbl.TrimEnd('s', 'S').ToLowerInvariant();
                return ($"{user} updated {cleanTableName} details", "update");
            }

            var actName = string.IsNullOrEmpty(action) ? "action" : action.ToLowerInvariant();
            var tblName = string.IsNullOrEmpty(tbl) ? "record" : tbl.TrimEnd('s', 'S').ToLowerInvariant();
            return ($"{user} performed {actName} on {tblName}", "info");
        }

        private static string GetRelativeTime(DateTime dateTime)
        {
            // Specify DateTimeKind.Utc so EF Core Unspecified datetime2 columns are correctly evaluated in UTC
            var utcCreated = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - utcCreated;

            if (timeSpan.TotalSeconds < 60)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hr ago";
            if (timeSpan.TotalDays < 2)
                return "Yesterday";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";

            return utcCreated.ToString("dd-MM-yyyy");
        }
    }
}
