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

        return services;
    }
}
