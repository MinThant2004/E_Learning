using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Database;

public partial class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<CourseExam> CourseExams { get; set; }
    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }
    public virtual DbSet<ExamQuestionOption> ExamQuestionOptions { get; set; }
    public virtual DbSet<ExamPayment> ExamPayments { get; set; }
    public virtual DbSet<CourseExamAttempt> CourseExamAttempts { get; set; }
    public virtual DbSet<CourseExamAttemptAnswer> CourseExamAttemptAnswers { get; set; }
    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
    public virtual DbSet<UserNotification> UserNotifications { get; set; }
    public virtual DbSet<Enrollment> Enrollments { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }
    public virtual DbSet<Quiz> Quizzes { get; set; }
    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }
    public virtual DbSet<QuizAnswer> QuizAnswers { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBD48B50706");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.Changes);
            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BEBDAF234");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A72F0FA64E");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BC3DB6EE9");
            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__B084ACD0F0BA05D8");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.LessonProgressId).HasName("PK__LessonPr__54391C1C97669B27");
            entity.ToTable("LessonProgress");
            entity.HasOne(d => d.Enrollment).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2FC7C78A6F");
            entity.HasIndex(e => e.PermissionCode, "UQ__Permissi__91FE575002860237").IsUnique();
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.PermissionCode).HasMaxLength(100);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.PasswordResetTokenId);
            entity.Property(e => e.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.VerifiedAt).IsRequired(false);
            entity.Property(e => e.UsedAt).IsRequired(false);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsUsed);
            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordResetTokens_Users");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FACF7BD404C");
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Question__92C7A1FF784A1790");
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quizzes__8B42AE8EADF40160");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__QuizAtte__891A68E69E71E3F9");
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuizAnswer>(entity =>
        {
            entity.HasKey(e => e.QuizAnswerId);
            entity.HasOne(d => d.Attempt).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Question).WithMany()
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.SelectedOption).WithMany()
                .HasForeignKey(d => d.SelectedOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId);
            entity.Property(e => e.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.RevokedAt).IsRequired(false);
            entity.Property(e => e.ReplacedByTokenId).IsRequired(false);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsRevoked);
            entity.Ignore(e => e.IsActive);
            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A8BE7CD7D");
            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616014AE9574").IsUnique();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__RolePerm__120F46BAE3BB2913");
            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CF4EA7EE4");
            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053474FE2317").IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.MustChangePassword).HasDefaultValue(false);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A35EE027180");
            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.UserRoleAssignedByNavigations).HasForeignKey(d => d.AssignedBy);
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CourseExam>(entity =>
        {
            entity.HasKey(e => e.ExamId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ExamFee).HasColumnType("decimal(18, 2)").IsRequired();
            entity.Property(e => e.MaxAttempts).HasDefaultValue(3);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Course).WithMany(p => p.CourseExams)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => e.ExamQuestionId);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.DifficultyLevel).HasMaxLength(50).HasDefaultValue("Medium");
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Exam).WithMany(p => p.ExamQuestions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamQuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId);
            entity.Property(e => e.OptionText).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasOne(d => d.Question).WithMany(p => p.ExamQuestionOptions)
                .HasForeignKey(d => d.ExamQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamPayment>(entity =>
        {
            entity.HasKey(e => e.ExamPaymentId);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)").IsRequired();
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TransactionId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ScreenshotUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.HasOne(d => d.Exam).WithMany(p => p.ExamPayments)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.ReviewedByNavigation).WithMany()
                .HasForeignKey(d => d.ReviewedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CourseExamAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId);
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("InProgress");
            entity.HasOne(d => d.CourseExam).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.ExamPayment).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.ExamPaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CourseExamAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.AttemptAnswerId);
            entity.Property(e => e.QuestionTextSnapshot).IsRequired();
            entity.HasOne(d => d.Attempt).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.ExamQuestion).WithMany()
                .HasForeignKey(d => d.ExamQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.SelectedOption).WithMany()
                .HasForeignKey(d => d.SelectedOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AccountName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.AccountNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.QrCodeUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(1);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("Info");
            entity.Property(e => e.TargetUrl).HasMaxLength(500).HasDefaultValue("/course-exams");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
