using ELearningManagementSystem.Application.Features.Authentication.Interfaces;
using ELearningManagementSystem.Application.Features.Authentication.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ELearningManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Authentication & Authorization Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ELearningManagementSystem.Application.Features.Permissions.Services.IPermissionManagementService, ELearningManagementSystem.Application.Features.Permissions.Services.PermissionManagementService>();
        services.AddScoped<ELearningManagementSystem.Application.Features.Roles.Services.IRoleService, ELearningManagementSystem.Application.Features.Roles.Services.RoleService>();
        services.AddScoped<ELearningManagementSystem.Application.Features.Users.Services.IUserService, ELearningManagementSystem.Application.Features.Users.Services.UserService>();

        // Course Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Courses.Services.ICourseService, ELearningManagementSystem.Application.Features.Courses.Services.CourseService>();

        // Category Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Categories.Services.ICategoryService, ELearningManagementSystem.Application.Features.Categories.Services.CategoryService>();

        // Lesson Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Lessons.Services.ILessonService, ELearningManagementSystem.Application.Features.Lessons.Services.LessonService>();

        // Enrollment Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Enrollments.Services.IEnrollmentService, ELearningManagementSystem.Application.Features.Enrollments.Services.EnrollmentService>();

        // Lesson Progress Services
        services.AddScoped<ELearningManagementSystem.Application.Features.LessonProgress.Services.ILessonProgressService, ELearningManagementSystem.Application.Features.LessonProgress.Services.LessonProgressService>();

        // Quiz Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Quizzes.Services.IQuizService, ELearningManagementSystem.Application.Features.Quizzes.Services.QuizService>();
        
        // Quiz Attempt Services
        services.AddScoped<ELearningManagementSystem.Application.Features.QuizAttempts.Services.IQuizAttemptService, ELearningManagementSystem.Application.Features.QuizAttempts.Services.QuizAttemptService>();

        // Course Exam Services
        services.AddScoped<ELearningManagementSystem.Application.Features.CourseExams.Services.ICourseExamService, ELearningManagementSystem.Application.Features.CourseExams.Services.CourseExamService>();

        // Exam Payment Services
        services.AddScoped<ELearningManagementSystem.Application.Features.ExamPayments.Services.IExamPaymentService, ELearningManagementSystem.Application.Features.ExamPayments.Services.ExamPaymentService>();

        // Exam Engine Services
        services.AddScoped<ELearningManagementSystem.Application.Features.ExamEngine.Services.IExamEngineService, ELearningManagementSystem.Application.Features.ExamEngine.Services.ExamEngineService>();

        // Payment Method Services
        services.AddScoped<ELearningManagementSystem.Application.Features.PaymentMethods.Services.IPaymentMethodService, ELearningManagementSystem.Application.Features.PaymentMethods.Services.PaymentMethodService>();

        // User Notification Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Notifications.Services.INotificationService, ELearningManagementSystem.Application.Features.Notifications.Services.NotificationService>();

        // Student Dashboard Services
        services.AddScoped<ELearningManagementSystem.Application.Features.StudentDashboard.Services.IStudentDashboardService, ELearningManagementSystem.Application.Features.StudentDashboard.Services.StudentDashboardService>();

        // Admin Dashboard Services
        services.AddScoped<ELearningManagementSystem.Application.Features.AdminDashboard.Services.IAdminDashboardService, ELearningManagementSystem.Application.Features.AdminDashboard.Services.AdminDashboardService>();

        // Audit Log Services
        services.AddScoped<ELearningManagementSystem.Application.Features.AuditLogs.Services.IAuditLogService, ELearningManagementSystem.Application.Features.AuditLogs.Services.AuditLogService>();

        // Report Services
        services.AddScoped<ELearningManagementSystem.Application.Features.Reports.Services.IReportService, ELearningManagementSystem.Application.Features.Reports.Services.ReportService>();

        return services;
    }
}
