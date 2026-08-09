using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.AdminDashboard.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        public async Task<Result<AdminDashboardResponse>> GetDashboardAsync()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Result.Failure<AdminDashboardResponse>("AccountNotFound: User identity not found.");
            }

            var permissions = await _permissionService.GetPermissionsForUserAsync(userId.Value);
            
            // If the user has NO read/write permissions for admin features, deny access to the dashboard.
            if (!permissions.Any(p => p == "User.Read" || p == "Role.Read" || p == "Category.Read" || p == "Course.Update" || p == "Lesson.Update" || p == "Quiz.Update"))
            {
                return Result.Failure<AdminDashboardResponse>("PermissionDenied: You do not have access to the admin dashboard.");
            }

            var response = new AdminDashboardResponse();

            // 1. Categories
            if (permissions.Contains("Category.Read"))
            {
                var activeCategories = await _context.Categories.CountAsync(c => !c.DeleteFlag);
                var archivedCategories = await _context.Categories.CountAsync(c => c.DeleteFlag);
                
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

            // 2. Courses
            if (permissions.Contains("Course.Read"))
            {
                var activeCourses = await _context.Courses.CountAsync(c => !c.DeleteFlag);
                var archivedCourses = await _context.Courses.CountAsync(c => c.DeleteFlag);

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

            // 3. Lessons
            if (permissions.Contains("Lesson.Read"))
            {
                var activeLessons = await _context.Lessons.CountAsync(l => !l.DeleteFlag);
                var archivedLessons = await _context.Lessons.CountAsync(l => l.DeleteFlag);

                response.Metrics.Add("Lessons", new ContentMetricResponse
                {
                    Title = "Lessons",
                    Icon = "document-text",
                    ActiveCount = activeLessons,
                    ArchivedCount = archivedLessons,
                    Route = "/admin/courses"
                });
            }

            // 4. Quizzes
            if (permissions.Contains("Quiz.Read"))
            {
                var activeQuizzes = await _context.Quizzes.CountAsync(q => !q.DeleteFlag);
                var archivedQuizzes = await _context.Quizzes.CountAsync(q => q.DeleteFlag);

                response.Metrics.Add("Quizzes", new ContentMetricResponse
                {
                    Title = "Quizzes",
                    Icon = "check-circle",
                    ActiveCount = activeQuizzes,
                    ArchivedCount = archivedQuizzes,
                    Route = "/admin/courses" // Assuming quizzes are managed from courses
                });
            }

            // 5. Users (Only if User.Read is granted)
            if (permissions.Contains("User.Read"))
            {
                var totalUsers = await _context.Users.CountAsync(); // Assuming no DeleteFlag for users or it's standard. We'll just count all for now.
                response.Metrics.Add("Users", new ContentMetricResponse
                {
                    Title = "Users",
                    Icon = "users",
                    ActiveCount = totalUsers,
                    ArchivedCount = 0,
                    Route = "/admin/users"
                });
            }

            return Result.Success(response);
        }
    }
}
