using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Category> Categories { get; }
    DbSet<Course> Courses { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
