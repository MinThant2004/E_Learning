using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ELearningManagementSystem.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Category> Categories { get; }
    DbSet<Course> Courses { get; }
    DbSet<CourseExam> CourseExams { get; }
    DbSet<ExamQuestion> ExamQuestions { get; }
    DbSet<ExamQuestionOption> ExamQuestionOptions { get; }
    DbSet<ExamPayment> ExamPayments { get; }
    DbSet<CourseExamAttempt> CourseExamAttempts { get; }
    DbSet<CourseExamAttemptAnswer> CourseExamAttemptAnswers { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<UserNotification> UserNotifications { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<QuizAnswer> QuizAnswers { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
