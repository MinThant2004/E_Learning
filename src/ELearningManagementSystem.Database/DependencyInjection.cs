using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELearningManagementSystem.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static void EnsurePhase23TablesCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<AppDbContext>();
        if (context == null) return;

        var sql = @"
        IF OBJECT_ID('dbo.ExamPayments') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExamPayments') AND name = N'ExamPaymentId')
        BEGIN
            ALTER TABLE dbo.ExamPayments DROP CONSTRAINT IF EXISTS [FK_ExamPayments_ExamAttempts];
            ALTER TABLE dbo.ExamAttempts DROP CONSTRAINT IF EXISTS [FK_ExamAttempts_ExamPayments];

            DROP TABLE IF EXISTS dbo.ExamAnswers;
            DROP TABLE IF EXISTS dbo.ExamAttemptOptionSnapshots;
            DROP TABLE IF EXISTS dbo.ExamAttemptQuestionSnapshots;
            DROP TABLE IF EXISTS dbo.Certificates;
            DROP TABLE IF EXISTS dbo.ExamAttempts;
            DROP TABLE IF EXISTS dbo.ExamPayments;
            DROP TABLE IF EXISTS dbo.ExamOptionPool;
            DROP TABLE IF EXISTS dbo.ExamQuestionPool;
            DROP TABLE IF EXISTS dbo.CertificationExams;
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseExams')
        BEGIN
            CREATE TABLE [dbo].[CourseExams] (
                [ExamId] INT IDENTITY(1,1) NOT NULL,
                [CourseId] INT NOT NULL,
                [Title] NVARCHAR(200) NOT NULL,
                [Description] NVARCHAR(MAX) NULL,
                [ExamFee] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                [QuestionCount] INT NOT NULL DEFAULT 50,
                [DurationMinutes] INT NOT NULL DEFAULT 60,
                [PassingScore] INT NOT NULL DEFAULT 70,
                [MaxAttempts] INT NOT NULL DEFAULT 3,
                [Status] BIT NOT NULL DEFAULT 1,
                [CreatedBy] INT NOT NULL,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_CourseExams] PRIMARY KEY CLUSTERED ([ExamId] ASC),
                CONSTRAINT [FK_CourseExams_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
                CONSTRAINT [FK_CourseExams_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserId])
            );
            CREATE NONCLUSTERED INDEX [IX_CourseExams_CourseId] ON [dbo].[CourseExams] ([CourseId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamQuestions')
        BEGIN
            CREATE TABLE [dbo].[ExamQuestions] (
                [ExamQuestionId] INT IDENTITY(1,1) NOT NULL,
                [ExamId] INT NOT NULL,
                [QuestionText] NVARCHAR(MAX) NOT NULL,
                [DisplayOrder] INT NOT NULL DEFAULT 1,
                [DifficultyLevel] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_ExamQuestions] PRIMARY KEY CLUSTERED ([ExamQuestionId] ASC),
                CONSTRAINT [FK_ExamQuestions_CourseExams] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[CourseExams] ([ExamId])
            );
            CREATE NONCLUSTERED INDEX [IX_ExamQuestions_ExamId] ON [dbo].[ExamQuestions] ([ExamId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamQuestionOptions')
        BEGIN
            CREATE TABLE [dbo].[ExamQuestionOptions] (
                [OptionId] INT IDENTITY(1,1) NOT NULL,
                [ExamQuestionId] INT NOT NULL,
                [OptionText] NVARCHAR(MAX) NOT NULL,
                [IsCorrect] BIT NOT NULL DEFAULT 0,
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_ExamQuestionOptions] PRIMARY KEY CLUSTERED ([OptionId] ASC),
                CONSTRAINT [FK_ExamQuestionOptions_ExamQuestions] FOREIGN KEY ([ExamQuestionId]) REFERENCES [dbo].[ExamQuestions] ([ExamQuestionId])
            );
            CREATE NONCLUSTERED INDEX [IX_ExamQuestionOptions_ExamQuestionId] ON [dbo].[ExamQuestionOptions] ([ExamQuestionId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamPayments')
        BEGIN
            CREATE TABLE [dbo].[ExamPayments] (
                [ExamPaymentId] INT IDENTITY(1,1) NOT NULL,
                [ExamId] INT NOT NULL,
                [UserId] INT NOT NULL,
                [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                [PaymentMethod] NVARCHAR(50) NOT NULL,
                [TransactionId] NVARCHAR(100) NOT NULL,
                [ScreenshotUrl] NVARCHAR(500) NOT NULL,
                [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                [IsUsed] BIT NOT NULL DEFAULT 0,
                [RejectionReason] NVARCHAR(500) NULL,
                [ReviewedBy] INT NULL,
                [ReviewedAt] DATETIME2 NULL,
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_ExamPayments] PRIMARY KEY CLUSTERED ([ExamPaymentId] ASC),
                CONSTRAINT [FK_ExamPayments_CourseExams] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[CourseExams] ([ExamId]),
                CONSTRAINT [FK_ExamPayments_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]),
                CONSTRAINT [FK_ExamPayments_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[Users] ([UserId])
            );
            CREATE NONCLUSTERED INDEX [IX_ExamPayments_ExamId_UserId] ON [dbo].[ExamPayments] ([ExamId], [UserId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseExamAttempts')
        BEGIN
            CREATE TABLE [dbo].[CourseExamAttempts] (
                [AttemptId] INT IDENTITY(1,1) NOT NULL,
                [ExamId] INT NOT NULL,
                [UserId] INT NOT NULL,
                [ExamPaymentId] INT NOT NULL,
                [StartedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [ExpiresAt] DATETIME2 NOT NULL,
                [SubmittedAt] DATETIME2 NULL,
                [Score] DECIMAL(5,2) NULL,
                [Passed] BIT NULL,
                [Status] NVARCHAR(50) NOT NULL DEFAULT 'InProgress',
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_CourseExamAttempts] PRIMARY KEY CLUSTERED ([AttemptId] ASC),
                CONSTRAINT [FK_CourseExamAttempts_CourseExams] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[CourseExams] ([ExamId]),
                CONSTRAINT [FK_CourseExamAttempts_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]),
                CONSTRAINT [FK_CourseExamAttempts_ExamPayments] FOREIGN KEY ([ExamPaymentId]) REFERENCES [dbo].[ExamPayments] ([ExamPaymentId])
            );
            CREATE NONCLUSTERED INDEX [IX_CourseExamAttempts_ExamId_UserId] ON [dbo].[CourseExamAttempts] ([ExamId], [UserId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseExamAttemptAnswers')
        BEGIN
            CREATE TABLE [dbo].[CourseExamAttemptAnswers] (
                [AttemptAnswerId] INT IDENTITY(1,1) NOT NULL,
                [AttemptId] INT NOT NULL,
                [ExamQuestionId] INT NOT NULL,
                [SelectedOptionId] INT NULL,
                [IsCorrect] BIT NULL,
                [QuestionOrder] INT NOT NULL DEFAULT 1,
                [QuestionTextSnapshot] NVARCHAR(MAX) NOT NULL,
                [ShuffledOptionsJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
                CONSTRAINT [PK_CourseExamAttemptAnswers] PRIMARY KEY CLUSTERED ([AttemptAnswerId] ASC),
                CONSTRAINT [FK_CourseExamAttemptAnswers_CourseExamAttempts] FOREIGN KEY ([AttemptId]) REFERENCES [dbo].[CourseExamAttempts] ([AttemptId]) ON DELETE CASCADE,
                CONSTRAINT [FK_CourseExamAttemptAnswers_ExamQuestions] FOREIGN KEY ([ExamQuestionId]) REFERENCES [dbo].[ExamQuestions] ([ExamQuestionId]),
                CONSTRAINT [FK_CourseExamAttemptAnswers_ExamQuestionOptions] FOREIGN KEY ([SelectedOptionId]) REFERENCES [dbo].[ExamQuestionOptions] ([OptionId])
            );
            CREATE NONCLUSTERED INDEX [IX_CourseExamAttemptAnswers_AttemptId] ON [dbo].[CourseExamAttemptAnswers] ([AttemptId]);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentMethods')
        BEGIN
            CREATE TABLE [dbo].[PaymentMethods] (
                [PaymentMethodId] INT IDENTITY(1,1) NOT NULL,
                [Code] NVARCHAR(50) NOT NULL,
                [Name] NVARCHAR(100) NOT NULL,
                [LogoUrl] NVARCHAR(500) NULL,
                [AccountName] NVARCHAR(150) NOT NULL DEFAULT 'EduSphere LMS',
                [AccountNumber] NVARCHAR(100) NOT NULL DEFAULT '09-789-012-345',
                [QrCodeUrl] NVARCHAR(500) NULL,
                [IsActive] BIT NOT NULL DEFAULT 1,
                [DisplayOrder] INT NOT NULL DEFAULT 1,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt] DATETIME2 NULL,
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_PaymentMethods] PRIMARY KEY CLUSTERED ([PaymentMethodId] ASC)
            );

            INSERT INTO [dbo].[PaymentMethods] ([Code], [Name], [LogoUrl], [AccountName], [AccountNumber], [IsActive], [DisplayOrder])
            VALUES 
            ('KPAY', 'KBZ Pay', '/images/payments/kpay.svg', 'EduSphere LMS', '09-789-012-345', 1, 1),
            ('AYAPAY', 'AYA Pay', '/images/payments/ayapay.svg', 'EduSphere LMS', '09-789-012-345', 1, 2),
            ('CBPAY', 'CB Pay', '/images/payments/cbpay.svg', 'EduSphere LMS', '0012-3456-7890', 1, 3),
            ('WAVEPAY', 'Wave Pay', '/images/payments/wavepay.svg', 'EduSphere LMS', '09-789-012-345', 1, 4);
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserNotifications')
        BEGIN
            CREATE TABLE [dbo].[UserNotifications] (
                [NotificationId] INT IDENTITY(1,1) NOT NULL,
                [UserId] INT NULL,
                [Title] NVARCHAR(200) NOT NULL,
                [Message] NVARCHAR(MAX) NOT NULL,
                [Type] NVARCHAR(50) NOT NULL DEFAULT 'Info',
                [TargetUrl] NVARCHAR(500) NOT NULL DEFAULT '/course-exams',
                [IsRead] BIT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [DeleteFlag] BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK_UserNotifications] PRIMARY KEY CLUSTERED ([NotificationId] ASC),
                CONSTRAINT [FK_UserNotifications_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
            );
            CREATE NONCLUSTERED INDEX [IX_UserNotifications_UserId] ON [dbo].[UserNotifications] ([UserId]);
        END";

        try
        {
            context.Database.ExecuteSqlRaw(sql);
        }
        catch
        {
            // Suppress error if database instance is not currently accessible
        }
    }
}
